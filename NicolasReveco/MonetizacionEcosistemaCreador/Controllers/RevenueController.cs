using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Registro y consulta de ingresos de un canal (RF-M3).</summary>
[ApiController]
[Route("api/[controller]")]
public class RevenueController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public RevenueController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M3: Registra un ingreso de un canal e incrementa su saldo de ganancias (crea el registro de ganancias si no existe).</summary>
    [HttpPost]
    public async Task<ActionResult<RevenueResponse>> Create([FromBody] CreateRevenueRequest request)
    {
        if (!Enum.TryParse<RevenueSource>(request.Source, ignoreCase: true, out var source))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                "El origen del ingreso no es válido.", new { allowed = Enum.GetNames<RevenueSource>() }));
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El monto del ingreso debe ser mayor que cero."));
        }

        if (request.PlatformSharePercent < 0 || request.PlatformSharePercent > 100)
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "PlatformSharePercent debe estar entre 0 y 100."));
        }

        if (!Enum.TryParse<RevenueStatus>(request.Status, ignoreCase: true, out var revenueStatus))
            revenueStatus = RevenueStatus.Pending;

        var platformShare = request.Amount * (request.PlatformSharePercent / 100m);
        var creatorShare = request.Amount - platformShare;
        var now = DateTime.UtcNow;

        var entry = new RevenueEntry
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            VideoId = request.VideoId,
            Source = source,
            Amount = request.Amount,
            PlatformShare = platformShare,
            CreatorShare = creatorShare,
            Status = revenueStatus,
            OccurredAt = request.OccurredAt ?? now
        };
        _db.RevenueEntries.Add(entry);

        // Only confirmed/paid revenue flows into the spendable balance.
        if (revenueStatus is RevenueStatus.Confirmed or RevenueStatus.Paid)
        {
            var earnings = await _db.CreatorEarnings.FirstOrDefaultAsync(x => x.ChannelId == request.ChannelId);
            if (earnings is null)
            {
                earnings = new CreatorEarnings
                {
                    Id = Guid.NewGuid(),
                    ChannelId = request.ChannelId,
                    Balance = 0m,
                    Currency = "USD"
                };
                _db.CreatorEarnings.Add(earnings);
            }

            earnings.Balance += creatorShare;
            earnings.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(List), new { channelId = entry.ChannelId }, ToResponse(entry));
    }

    /// <summary>RF-M3: Lista paginada de ingresos de un canal, opcionalmente filtrados por origen.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<RevenueResponse>>> List(
        [FromQuery] Guid channelId,
        [FromQuery] string? source,
        [FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.RevenueEntries.AsNoTracking().Where(r => r.ChannelId == channelId);

        if (!string.IsNullOrWhiteSpace(source))
        {
            if (!Enum.TryParse<RevenueSource>(source, ignoreCase: true, out var src))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    "El origen del ingreso no es válido.", new { allowed = Enum.GetNames<RevenueSource>() }));
            }
            baseQuery = baseQuery.Where(r => r.Source == src);
        }

        var total = await baseQuery.CountAsync();
        var entities = await baseQuery
            .OrderByDescending(r => r.OccurredAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var items = entities.Select(ToResponse).ToList();

        return Ok(PagedResult<RevenueResponse>.Create(items, query.Page, query.PageSize, total));
    }

    private static RevenueResponse ToResponse(RevenueEntry r) => new()
    {
        Id = r.Id,
        ChannelId = r.ChannelId,
        VideoId = r.VideoId,
        Source = r.Source.ToString(),
        Amount = r.Amount,
        PlatformShare = r.PlatformShare,
        CreatorShare = r.CreatorShare,
        Status = r.Status.ToString(),
        OccurredAt = r.OccurredAt
    };
}
