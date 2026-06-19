using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D3 Contenido relacionado: encuentra contenido que comparte etiquetas con uno dado.</summary>
[ApiController]
[Route("api/[controller]")]
public class ContentController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public ContentController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D3: Devuelve el contenido relacionado al indicado: comparte al menos una etiqueta con el
    /// contenido base, excluyéndolo a sí mismo, ordenado por RankingScore descendente. Devuelve solo
    /// referencias (la metadata se hidrata desde Catálogo). 404 si el contenido base no está indexado.
    /// </summary>
    [HttpGet("{id:guid}/related")]
    [ProducesResponseType(typeof(PagedResult<SearchResultItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<SearchResultItem>>> GetRelated(
        Guid id,
        [FromQuery] PaginationQuery query)
    {
        var baseContent = await _db.IndexedContents
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (baseContent is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Contenido no encontrado en el índice."));

        var baseTags = baseContent.Tags.Select(t => t.Value).ToList();

        if (baseTags.Count == 0)
            return Ok(PagedResult<SearchResultItem>.Create(new List<SearchResultItem>(), query.Page, query.PageSize, 0));

        var baseQuery = _db.IndexedContents
            .Where(c => c.Id != id && c.Tags.Any(t => baseTags.Contains(t.Value)))
            .OrderByDescending(c => c.RankingScore)
            .ThenByDescending(c => c.UpdatedAt);

        var total = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(c => new SearchResultItem
            {
                ContentId = c.Id,
                ChannelId = c.ChannelId,
                Title = c.Title,
                RankingScore = c.RankingScore,
                Tags = c.Tags.Select(t => t.Value).ToList()
            })
            .ToListAsync();

        return Ok(PagedResult<SearchResultItem>.Create(items, query.Page, query.PageSize, total));
    }
}
