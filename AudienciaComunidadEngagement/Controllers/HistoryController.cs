using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A5: historial de visualización de los usuarios.</summary>
[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public HistoryController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A5: registra la visualización (upsert por user+item, actualizando PositionSeconds y WatchedAt).</summary>
    [HttpPost]
    public async Task<ActionResult<WatchHistoryEntryDto>> Record([FromBody] RecordWatchDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.CatalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId y CatalogItemId son obligatorios."));
        if (dto.PositionSeconds < 0)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "PositionSeconds no puede ser negativo."));

        var existing = await _db.WatchHistoryEntries
            .FirstOrDefaultAsync(h => h.UserId == dto.UserId && h.CatalogItemId == dto.CatalogItemId);

        if (existing is not null)
        {
            existing.PositionSeconds = dto.PositionSeconds;
            existing.WatchedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(ToDto(existing));
        }

        var entity = new WatchHistoryEntry
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            CatalogItemId = dto.CatalogItemId,
            PositionSeconds = dto.PositionSeconds,
            WatchedAt = DateTime.UtcNow
        };
        _db.WatchHistoryEntries.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A5: obtiene una entrada de historial por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WatchHistoryEntryDto>> GetById(Guid id)
    {
        var entity = await _db.WatchHistoryEntries.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Entrada de historial no encontrada."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A5: lista el historial de un usuario, paginado y ordenado por visualización más reciente.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<WatchHistoryEntryDto>>> List(
        [FromQuery] Guid userId,
        [FromQuery] PaginationQuery query)
    {
        if (userId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "userId es obligatorio."));

        var q = _db.WatchHistoryEntries.Where(h => h.UserId == userId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(h => h.WatchedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(h => ToDto(h)).ToListAsync();

        return Ok(PagedResult<WatchHistoryEntryDto>.Create(items, query.Page, query.PageSize, total));
    }

    private static WatchHistoryEntryDto ToDto(WatchHistoryEntry h) => new()
    {
        Id = h.Id,
        UserId = h.UserId,
        CatalogItemId = h.CatalogItemId,
        PositionSeconds = h.PositionSeconds,
        WatchedAt = h.WatchedAt
    };
}
