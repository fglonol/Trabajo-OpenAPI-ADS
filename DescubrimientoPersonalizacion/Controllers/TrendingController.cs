using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D4 Tendencias: lista el contenido indexado ordenado por su puntaje de tendencia.</summary>
[ApiController]
[Route("api/[controller]")]
public class TrendingController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public TrendingController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D4: Devuelve el contenido en tendencia, ordenado por TrendingScore descendente. Devuelve solo
    /// referencias (la metadata se hidrata desde Catálogo).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TrendingItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TrendingItem>>> GetTrending(
        [FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.IndexedContents
            .OrderByDescending(c => c.TrendingScore)
            .ThenByDescending(c => c.ImpressionCount);

        var total = await baseQuery.CountAsync();

        var items = await baseQuery
            .Skip(query.Skip)
            .Take(query.PageSize)
            .Select(c => new TrendingItem
            {
                ContentId = c.Id,
                ChannelId = c.ChannelId,
                Title = c.Title,
                TrendingScore = c.TrendingScore,
                ImpressionCount = c.ImpressionCount,
                Tags = c.Tags.Select(t => t.Value).ToList()
            })
            .ToListAsync();

        return Ok(PagedResult<TrendingItem>.Create(items, query.Page, query.PageSize, total));
    }
}
