using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A4: gestión de notificaciones dirigidas a los usuarios.</summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public NotificationsController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A4: crea una notificación para un usuario.</summary>
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDto dto)
    {
        if (dto.UserId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId es obligatorio."));
        if (string.IsNullOrWhiteSpace(dto.Type))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Type es obligatorio."));

        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Type = dto.Type,
            Message = dto.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A4: obtiene una notificación por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationDto>> GetById(Guid id)
    {
        var entity = await _db.Notifications.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Notificación no encontrada."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A4: lista notificaciones filtrando por userId y/o estado isRead, paginado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationDto>>> List(
        [FromQuery] Guid? userId,
        [FromQuery] bool? isRead,
        [FromQuery] PaginationQuery query)
    {
        var q = _db.Notifications.AsQueryable();
        if (userId.HasValue) q = q.Where(n => n.UserId == userId.Value);
        if (isRead.HasValue) q = q.Where(n => n.IsRead == isRead.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(n => n.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(n => ToDto(n)).ToListAsync();

        return Ok(PagedResult<NotificationDto>.Create(items, query.Page, query.PageSize, total));
    }

    /// <summary>RF-A4: marca una notificación como leída (IsRead = true).</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead(Guid id)
    {
        var entity = await _db.Notifications.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Notificación no encontrada."));

        entity.IsRead = true;
        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    /// <summary>RF-A4: marca como leídas todas las notificaciones de un usuario.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid userId)
    {
        if (userId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "userId es obligatorio."));

        var pending = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
        foreach (var n in pending)
            n.IsRead = true;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static NotificationDto ToDto(Notification n) => new()
    {
        Id = n.Id,
        UserId = n.UserId,
        Type = n.Type,
        Message = n.Message,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
