using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A1: gestión de suscripciones de usuarios a canales.</summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public SubscriptionsController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A1: crea una suscripción. 409 si el usuario ya está suscrito al canal.</summary>
    [HttpPost]
    public async Task<ActionResult<SubscriptionDto>> Create([FromBody] CreateSubscriptionDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.ChannelId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId y ChannelId son obligatorios."));

        var exists = await _db.Subscriptions
            .AnyAsync(s => s.UserId == dto.UserId && s.ChannelId == dto.ChannelId);
        if (exists)
            return Conflict(new ErrorResponse("CONFLICT", "El usuario ya está suscrito a este canal."));

        var entity = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            ChannelId = dto.ChannelId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Subscriptions.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A1: obtiene una suscripción por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubscriptionDto>> GetById(Guid id)
    {
        var entity = await _db.Subscriptions.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Suscripción no encontrada."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A1: elimina (cancela) una suscripción.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Subscriptions.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Suscripción no encontrada."));
        _db.Subscriptions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-A1: lista suscripciones filtrando por userId o channelId, paginado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<SubscriptionDto>>> List(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? channelId,
        [FromQuery] PaginationQuery query)
    {
        var q = _db.Subscriptions.AsQueryable();
        if (userId.HasValue) q = q.Where(s => s.UserId == userId.Value);
        if (channelId.HasValue) q = q.Where(s => s.ChannelId == channelId.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(s => s.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(s => ToDto(s)).ToListAsync();

        return Ok(PagedResult<SubscriptionDto>.Create(items, query.Page, query.PageSize, total));
    }

    /// <summary>RF-A1: cantidad de suscriptores de un canal (consumido por Monetización).</summary>
    [HttpGet("count")]
    public async Task<ActionResult<SubscriberCountDto>> Count([FromQuery] Guid channelId)
    {
        if (channelId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "channelId es obligatorio."));
        var count = await _db.Subscriptions.CountAsync(s => s.ChannelId == channelId);
        return Ok(new SubscriberCountDto { ChannelId = channelId, Count = count });
    }

    private static SubscriptionDto ToDto(Subscription s) => new()
    {
        Id = s.Id,
        UserId = s.UserId,
        ChannelId = s.ChannelId,
        CreatedAt = s.CreatedAt
    };
}
