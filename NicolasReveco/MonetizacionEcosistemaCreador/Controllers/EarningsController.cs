using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Consulta de ganancias de un canal con desglose por origen (RF-M4).</summary>
[ApiController]
[Route("api/[controller]")]
public class EarningsController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public EarningsController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M4: Obtiene ganancias por periodo y desglose por origen/video. Soporta groupBy (source|video|day|month) y format (json|csv).</summary>
    [HttpGet("{channelId:guid}")]
    public async Task<ActionResult<EarningsResponse>> GetByChannel(
        Guid channelId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string groupBy = "source",
        [FromQuery] string format = "json")
    {
        var earnings = await _db.CreatorEarnings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ChannelId == channelId);
        if (earnings is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "El canal no tiene un registro de ganancias."));
        }

        var query = _db.RevenueEntries.AsNoTracking().Where(r => r.ChannelId == channelId);
        if (from is not null) query = query.Where(r => r.OccurredAt >= from.Value);
        if (to is not null)   query = query.Where(r => r.OccurredAt <= to.Value);

        var entries = await query.ToListAsync();

        var breakdown = groupBy.ToLowerInvariant() switch
        {
            "video" => entries
                .GroupBy(r => r.VideoId?.ToString() ?? "none")
                .Select(g => new RevenueBreakdownItem
                {
                    VideoId = g.First().VideoId,
                    Total = g.Sum(x => x.CreatorShare)
                }),
            "day" => entries
                .GroupBy(r => r.OccurredAt.ToString("yyyy-MM-dd"))
                .Select(g => new RevenueBreakdownItem
                {
                    Period = g.Key,
                    Total = g.Sum(x => x.CreatorShare)
                }),
            "month" => entries
                .GroupBy(r => r.OccurredAt.ToString("yyyy-MM"))
                .Select(g => new RevenueBreakdownItem
                {
                    Period = g.Key,
                    Total = g.Sum(x => x.CreatorShare)
                }),
            _ => entries
                .GroupBy(r => r.Source)
                .Select(g => new RevenueBreakdownItem
                {
                    Source = g.Key.ToString(),
                    Total = g.Sum(x => x.CreatorShare)
                })
        };

        var totalCreatorRevenue = entries.Sum(r => r.CreatorShare);
        var rpm = entries.Count > 0 ? totalCreatorRevenue / entries.Count * 1000m : 0m;

        var response = new EarningsResponse
        {
            Id = earnings.Id,
            ChannelId = earnings.ChannelId,
            CreatorUserId = earnings.CreatorUserId,
            Balance = earnings.Balance,
            Currency = earnings.Currency,
            UpdatedAt = earnings.UpdatedAt,
            From = from,
            To = to,
            Rpm = (double)rpm,
            ExportUrl = format.Equals("csv", StringComparison.OrdinalIgnoreCase)
                ? $"/api/earnings/{channelId}/export.csv"
                : null,
            Breakdown = breakdown.ToList()
        };

        return Ok(response);
    }
}
