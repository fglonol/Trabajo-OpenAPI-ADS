using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>
/// Reporting de campañas (RF-F8): consolida las métricas medidas (impresiones, clics, CTR,
/// gasto y presupuesto restante) a partir de los eventos registrados.
/// </summary>
[ApiController]
[Route("api/reporting")]
public class ReportingController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public ReportingController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F8: reporte de desempeño de una campaña (impresiones, clics, CTR, gasto, presupuesto restante).</summary>
    [HttpGet("campaigns/{id:guid}")]
    [ProducesResponseType(typeof(CampaignReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampaignReport(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        var impressions = await _db.AdEvents
            .CountAsync(e => e.CampaignId == id && e.Type == AdEventType.Impression);
        var clicks = await _db.AdEvents
            .CountAsync(e => e.CampaignId == id && e.Type == AdEventType.Click);
        var skips = await _db.AdEvents
            .CountAsync(e => e.CampaignId == id && e.Type == AdEventType.Skip);

        var ctr = impressions > 0 ? clicks / (double)impressions : 0d;
        var cpm = impressions > 0 ? (double)campaign.Spend / impressions * 1000d : 0d;

        // No existe en el OpenAPI un endpoint de creación/asociación de invoices a campañas, por lo
        // que invoiceId queda en null hasta que ese mecanismo se defina en otro bounded context.
        var report = new CampaignReportResponse
        {
            CampaignId = campaign.Id,
            Impressions = impressions,
            Clicks = clicks,
            Skips = skips,
            Ctr = ctr,
            Cpm = cpm,
            Spend = campaign.Spend,
            RemainingBudget = campaign.Budget - campaign.Spend,
            InvoiceId = null
        };

        return Ok(report);
    }

    /// <summary>RF-F8: lista las invoices del anunciante, opcionalmente filtradas por periodo.</summary>
    [HttpGet("invoices/{advertiserId:guid}")]
    [ProducesResponseType(typeof(PagedResult<InvoiceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListInvoices(
        Guid advertiserId,
        [FromQuery] PaginationQuery query,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var advertiserExists = await _db.Advertisers.AnyAsync(a => a.Id == advertiserId);
        if (!advertiserExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Anunciante no encontrado."));
        }

        var baseQuery = _db.Invoices.Where(i => i.AdvertiserId == advertiserId);

        if (from.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.PeriodEnd >= from.Value);
        }

        if (to.HasValue)
        {
            baseQuery = baseQuery.Where(i => i.PeriodStart <= to.Value);
        }

        var totalItems = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(i => i.PeriodStart)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToInvoiceResponse).ToList();
        return Ok(PagedResult<InvoiceResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    private static InvoiceResponse ToInvoiceResponse(Invoice i) => new()
    {
        Id = i.Id,
        AdvertiserId = i.AdvertiserId,
        PeriodStart = i.PeriodStart,
        PeriodEnd = i.PeriodEnd,
        Impressions = i.Impressions,
        Clicks = i.Clicks,
        Cpm = i.Cpm,
        TotalSpend = i.TotalSpend,
        Status = i.Status.ToString(),
        IssuedAt = i.IssuedAt
    };
}
