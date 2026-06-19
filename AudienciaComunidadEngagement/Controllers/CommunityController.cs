using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A6: gestión de publicaciones de comunidad de los canales.</summary>
[ApiController]
[Route("api/[controller]")]
public class CommunityController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public CommunityController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A6: crea una publicación de comunidad para un canal.</summary>
    [HttpPost]
    public async Task<ActionResult<CommunityPostDto>> Create([FromBody] CreateCommunityPostDto dto)
    {
        if (dto.ChannelId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "ChannelId es obligatorio."));
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El texto de la publicación es obligatorio."));

        var entity = new CommunityPost
        {
            Id = Guid.NewGuid(),
            ChannelId = dto.ChannelId,
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow
        };
        _db.CommunityPosts.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A6: obtiene una publicación de comunidad por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommunityPostDto>> GetById(Guid id)
    {
        var entity = await _db.CommunityPosts.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Publicación de comunidad no encontrada."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A6: elimina una publicación de comunidad.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.CommunityPosts.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Publicación de comunidad no encontrada."));
        _db.CommunityPosts.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-A6: lista publicaciones de comunidad de un canal, paginado y ordenado por más reciente.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommunityPostDto>>> List(
        [FromQuery] Guid channelId,
        [FromQuery] PaginationQuery query)
    {
        if (channelId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "channelId es obligatorio."));

        var q = _db.CommunityPosts.Where(p => p.ChannelId == channelId);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(p => ToDto(p)).ToListAsync();

        return Ok(PagedResult<CommunityPostDto>.Create(items, query.Page, query.PageSize, total));
    }

    private static CommunityPostDto ToDto(CommunityPost p) => new()
    {
        Id = p.Id,
        ChannelId = p.ChannelId,
        Text = p.Text,
        CreatedAt = p.CreatedAt
    };
}
