using AudienciaComunidadEngagement.Common;
using AudienciaComunidadEngagement.Data;
using AudienciaComunidadEngagement.Models;
using AudienciaComunidadEngagement.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AudienciaComunidadEngagement.Controllers;

/// <summary>RF-A3: gestión de comentarios de usuarios sobre items del catálogo, con respuestas anidadas.</summary>
[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly AudienciaComunidadEngagementDbContext _db;

    public CommentsController(AudienciaComunidadEngagementDbContext db) => _db = db;

    /// <summary>RF-A3: crea un comentario (o respuesta si se indica ParentCommentId).</summary>
    [HttpPost]
    public async Task<ActionResult<CommentDto>> Create([FromBody] CreateCommentDto dto)
    {
        if (dto.UserId == Guid.Empty || dto.CatalogItemId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "UserId y CatalogItemId son obligatorios."));
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El texto del comentario es obligatorio."));

        if (dto.ParentCommentId.HasValue)
        {
            var parentExists = await _db.Comments.AnyAsync(c => c.Id == dto.ParentCommentId.Value);
            if (!parentExists)
                return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El comentario padre no existe."));
        }

        var entity = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            CatalogItemId = dto.CatalogItemId,
            ParentCommentId = dto.ParentCommentId,
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
    }

    /// <summary>RF-A3: obtiene un comentario por su Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CommentDto>> GetById(Guid id)
    {
        var entity = await _db.Comments.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Comentario no encontrado."));
        return Ok(ToDto(entity));
    }

    /// <summary>RF-A3: edita el texto de un comentario y registra EditedAt.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CommentDto>> Update(Guid id, [FromBody] UpdateCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El texto del comentario es obligatorio."));

        var entity = await _db.Comments.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Comentario no encontrado."));

        entity.Text = dto.Text;
        entity.EditedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(ToDto(entity));
    }

    /// <summary>RF-A3: elimina un comentario.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Comments.FindAsync(id);
        if (entity is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Comentario no encontrado."));
        _db.Comments.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-A3: lista comentarios filtrando por catalogItemId y/o parentCommentId, paginado.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommentDto>>> List(
        [FromQuery] Guid? catalogItemId,
        [FromQuery] Guid? parentCommentId,
        [FromQuery] PaginationQuery query)
    {
        var q = _db.Comments.AsQueryable();
        if (catalogItemId.HasValue) q = q.Where(c => c.CatalogItemId == catalogItemId.Value);
        if (parentCommentId.HasValue) q = q.Where(c => c.ParentCommentId == parentCommentId.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(c => c.CreatedAt)
            .Skip(query.Skip).Take(query.PageSize)
            .Select(c => ToDto(c)).ToListAsync();

        return Ok(PagedResult<CommentDto>.Create(items, query.Page, query.PageSize, total));
    }

    private static CommentDto ToDto(Comment c) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        CatalogItemId = c.CatalogItemId,
        ParentCommentId = c.ParentCommentId,
        Text = c.Text,
        CreatedAt = c.CreatedAt,
        EditedAt = c.EditedAt
    };
}
