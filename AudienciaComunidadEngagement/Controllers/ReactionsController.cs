using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A2: gestión de reacciones (like/dislike) de usuarios sobre items del catálogo.</summary>
[ApiController]
[Route("api/[controller]")]
public class ReactionsController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public ReactionsController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A2: upsert de una reacción. Si (UserId, CatalogItemId) ya reaccionó, actualiza el Type; si no, la crea.</summary>
    [HttpPost]
    public async Task<ActionResult<ReactionDto>> Upsert([FromBody] CreateReactionDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.CatalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId y CatalogItemId son obligatorios."));

        var existing = await _db.Reactions
            .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.CatalogItemId == dto.CatalogItemId);

        if (existing is not null)
        {
            existing.Type = dto.Type;
            await _db.SaveChangesAsync();
            return Ok(ToDto(existing));
        }

        var entity = new Reaction
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            CatalogItemId = dto.CatalogItemId,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow
        };
        _db.Reactions.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A2: obtiene una reacción por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReactionDto>> GetById(Guid id)
    {
        var entity = await _db.Reactions.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Reacción no encontrada."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A2: elimina (quita) una reacción.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Reactions.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Reacción no encontrada."));
        _db.Reactions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-A2: lista reacciones de un item del catálogo, paginado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReactionDto>>> List(
        [FromQuery] Guid catalogItemId,
        [FromQuery] PaginationQuery query)
    {
        if (catalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "catalogItemId es obligatorio."));

        var q = _db.Reactions.Where(r => r.CatalogItemId == catalogItemId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(r => r.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(r => ToDto(r)).ToListAsync();

        return Ok(PagedResult<ReactionDto>.Create(items, query.Page, query.PageSize, total));
    }

    /// <summary>RF-A2: resumen de reacciones de un item (conteo de likes y dislikes).</summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ReactionSummaryDto>> Summary([FromQuery] Guid catalogItemId)
    {
        if (catalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "catalogItemId es obligatorio."));

        var likes = await _db.Reactions.CountAsync(r => r.CatalogItemId == catalogItemId && r.Type == ReactionType.Like);
        var dislikes = await _db.Reactions.CountAsync(r => r.CatalogItemId == catalogItemId && r.Type == ReactionType.Dislike);

        return Ok(new ReactionSummaryDto { CatalogItemId = catalogItemId, Likes = likes, Dislikes = dislikes });
    }

    private static ReactionDto ToDto(Reaction r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        CatalogItemId = r.CatalogItemId,
        Type = r.Type,
        CreatedAt = r.CreatedAt
    };
}
