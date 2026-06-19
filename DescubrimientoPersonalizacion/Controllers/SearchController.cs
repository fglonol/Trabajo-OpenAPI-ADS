using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D1 Búsqueda: busca contenido indexado por título o etiquetas.</summary>
[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public SearchController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D1: Busca contenido cuyo título o alguna etiqueta contengan el término <paramref name="q"/>,
    /// ordenado por RankingScore descendente. Devuelve solo referencias (la metadata se hidrata desde Catálogo).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SearchResultItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<SearchResultItem>>> Search(
        [FromQuery] string? q,
        [FromQuery] PaginationQuery query)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El parámetro de búsqueda 'q' es obligatorio."));

        var term = q.Trim();

        var baseQuery = _db.IndexedContents
            .Where(c => EF.Functions.Like(c.Title, $"%{term}%")
                     || c.Tags.Any(t => EF.Functions.Like(t.Value, $"%{term}%")))
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
