using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D6 Indexación: mantiene las referencias indexadas del contenido publicado.</summary>
[ApiController]
[Route("api/[controller]")]
public class IndexController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public IndexController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D6: Upsert de un contenido en el índice. Si el Id existe se actualiza, si no se inserta.
    /// El Id es el catalogItemId proveniente de Catálogo (no se genera aquí).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IndexedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IndexedContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IndexedContentResponse>> Upsert([FromBody] IndexContentRequest request)
    {
        if (request.Id == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El Id (catalogItemId) es obligatorio."));
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El título es obligatorio."));

        var now = DateTime.UtcNow;
        var tags = (request.Tags ?? new List<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existing = await _db.IndexedContents
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == request.Id);

        bool created = existing is null;

        if (created)
        {
            existing = new IndexedContent
            {
                Id = request.Id,
                CreatedAt = now,
                RankingScore = 0,
                TrendingScore = 0,
                ImpressionCount = 0
            };
            _db.IndexedContents.Add(existing);
        }

        existing!.ChannelId = request.ChannelId;
        existing.Title = request.Title.Trim();
        existing.UpdatedAt = now;

        // Reemplaza el conjunto de etiquetas.
        if (existing.Tags.Count > 0)
            _db.ContentTags.RemoveRange(existing.Tags);
        existing.Tags = tags.Select(value => new ContentTag
        {
            Id = Guid.NewGuid(),
            IndexedContentId = existing.Id,
            Value = value
        }).ToList();

        await _db.SaveChangesAsync();

        var response = ToResponse(existing);
        return created
            ? CreatedAtAction(nameof(GetById), new { id = existing.Id }, response)
            : Ok(response);
    }

    /// <summary>RF-D6: Elimina un contenido del índice (usado cuando el contenido se despublica).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _db.IndexedContents.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Contenido no encontrado en el índice."));

        _db.IndexedContents.Remove(existing); // cascada borra ContentTags
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-D6: Obtiene un contenido indexado por su Id (catalogItemId).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IndexedContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IndexedContentResponse>> GetById(Guid id)
    {
        var content = await _db.IndexedContents
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (content is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Contenido no encontrado en el índice."));

        return Ok(ToResponse(content));
    }

    private static IndexedContentResponse ToResponse(IndexedContent c) => new()
    {
        Id = c.Id,
        ChannelId = c.ChannelId,
        Title = c.Title,
        RankingScore = c.RankingScore,
        TrendingScore = c.TrendingScore,
        ImpressionCount = c.ImpressionCount,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Tags = c.Tags.Select(t => t.Value).ToList()
    };
}
