using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>
/// Medición publicitaria (RF-F7): registra eventos de impresión y clic, acumula el gasto
/// de la campaña y la completa automáticamente al agotar el presupuesto.
/// </summary>
[ApiController]
[Route("api/measurement")]
public class MeasurementController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public MeasurementController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F7: registra un evento de medición (Impression | Click) e incrementa el gasto de la campaña.</summary>
    [HttpPost("events")]
    [ProducesResponseType(typeof(AdEventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordEvent([FromBody] CreateAdEventRequest request)
    {
        if (!Enum.TryParse<AdEventType>(request.Type, ignoreCase: true, out var eventType))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Tipo de evento inválido: '{request.Type}'.",
                new { allowed = Enum.GetNames<AdEventType>() }));
        }

        if (request.CostPerEvent < 0)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                "El costo por evento no puede ser negativo."));
        }

        var campaign = await _db.AdCampaigns.FindAsync(request.CampaignId);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        var creativeExists = await _db.Creatives
            .AnyAsync(c => c.Id == request.CreativeId && c.CampaignId == request.CampaignId);
        if (!creativeExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND",
                "Creativo no encontrado para la campaña indicada."));
        }

        var adEvent = new AdEvent
        {
            Id = Guid.NewGuid(),
            CampaignId = request.CampaignId,
            CreativeId = request.CreativeId,
            CatalogItemId = request.CatalogItemId,
            Type = eventType,
            CostPerEvent = request.CostPerEvent,
            OccurredAt = DateTime.UtcNow
        };

        _db.AdEvents.Add(adEvent);

        // RF-F7: el evento acumula gasto en la campaña.
        campaign.Spend += request.CostPerEvent;

        // Presupuesto agotado -> la campaña se completa (consistencia con RF-F2).
        if (campaign.Spend >= campaign.Budget && campaign.Status == CampaignStatus.Active)
        {
            campaign.Status = CampaignStatus.Completed;
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = adEvent.Id }, ToResponse(adEvent));
    }

    /// <summary>RF-F7: obtiene un evento de medición por su Id.</summary>
    [HttpGet("events/{id:guid}")]
    [ProducesResponseType(typeof(AdEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(Guid id)
    {
        var adEvent = await _db.AdEvents.FindAsync(id);
        if (adEvent is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Evento no encontrado."));
        }

        return Ok(ToResponse(adEvent));
    }

    /// <summary>RF-F7: listado paginado de eventos, con filtros opcionales por campaña y tipo.</summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(PagedResult<AdEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListEvents(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? campaignId,
        [FromQuery] string? type)
    {
        var baseQuery = _db.AdEvents.AsQueryable();

        if (campaignId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.CampaignId == campaignId.Value);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<AdEventType>(type, ignoreCase: true, out var parsed))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    $"Tipo de evento inválido: '{type}'.",
                    new { allowed = Enum.GetNames<AdEventType>() }));
            }

            baseQuery = baseQuery.Where(e => e.Type == parsed);
        }

        var totalItems = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(e => e.OccurredAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToResponse).ToList();
        return Ok(PagedResult<AdEventResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    private static AdEventResponse ToResponse(AdEvent e) => new()
    {
        Id = e.Id,
        CampaignId = e.CampaignId,
        CreativeId = e.CreativeId,
        CatalogItemId = e.CatalogItemId,
        Type = e.Type.ToString(),
        CostPerEvent = e.CostPerEvent,
        OccurredAt = e.OccurredAt
    };
}
