using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A5: gestión de la lista "ver más tarde" de los usuarios.</summary>
[ApiController]
[Route("api/[controller]")]
public class WatchLaterController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public WatchLaterController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A5: agrega un item a "ver más tarde". 409 si el usuario ya lo tiene.</summary>
    [HttpPost]
    public async Task<ActionResult<WatchLaterItemDto>> Create([FromBody] CreateWatchLaterDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.CatalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId y CatalogItemId son obligatorios."));

        var exists = await _db.WatchLaterItems
            .AnyAsync(w => w.UserId == dto.UserId && w.CatalogItemId == dto.CatalogItemId);
        if (exists)
            return Conflict(new ErrorResponse("CONFLICT", "El item ya está en la lista 'ver más tarde' del usuario."));

        var entity = new WatchLaterItem
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            CatalogItemId = dto.CatalogItemId,
            AddedAt = DateTime.UtcNow
        };
        _db.WatchLaterItems.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A5: obtiene un item de "ver más tarde" por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WatchLaterItemDto>> GetById(Guid id)
    {
        var entity = await _db.WatchLaterItems.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Item de 'ver más tarde' no encontrado."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A5: elimina un item de la lista "ver más tarde".</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.WatchLaterItems.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Item de 'ver más tarde' no encontrado."));
        _db.WatchLaterItems.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-A5: lista los items "ver más tarde" de un usuario, paginado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<WatchLaterItemDto>>> List(
        [FromQuery] Guid userId,
        [FromQuery] PaginationQuery query)
    {
        if (userId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "userId es obligatorio."));

        var q = _db.WatchLaterItems.Where(w => w.UserId == userId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(w => w.AddedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(w => ToDto(w)).ToListAsync();

        return Ok(PagedResult<WatchLaterItemDto>.Create(items, query.Page, query.PageSize, total));
    }

    private static WatchLaterItemDto ToDto(WatchLaterItem w) => new()
    {
        Id = w.Id,
        UserId = w.UserId,
        CatalogItemId = w.CatalogItemId,
        AddedAt = w.AddedAt
    };
}
