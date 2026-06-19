using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D2 Feed personalizado: rankea el contenido indexado boosteando los intereses del usuario.</summary>
[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public FeedController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D2: Devuelve el feed personalizado del usuario. Ordena el contenido indexado por un puntaje
    /// personalizado = RankingScore + suma de los Score de los UserInterest cuyo Interest coincide con
    /// alguna etiqueta del contenido. Devuelve solo referencias (la metadata se hidrata desde Catálogo).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FeedItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<FeedItem>>> GetFeed(
        [FromQuery] Guid userId,
        [FromQuery] PaginationQuery query)
    {
        if (userId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El parámetro 'userId' es obligatorio."));

        // Intereses del usuario: mapa etiqueta -> puntaje de boost.
        var interests = await _db.UserInterests
            .Where(i => i.UserId == userId)
            .ToDictionaryAsync(i => i.Interest, i => i.Score, StringComparer.OrdinalIgnoreCase);

        // Se traen las referencias indexadas con sus etiquetas para calcular el boost en memoria.
        var contents = await _db.IndexedContents
            .Include(c => c.Tags)
            .ToListAsync();

        var ranked = contents
            .Select(c =>
            {
                var boost = c.Tags
                    .Where(t => interests.ContainsKey(t.Value))
                    .Sum(t => interests[t.Value]);

                return new FeedItem
                {
                    ContentId = c.Id,
                    ChannelId = c.ChannelId,
                    Title = c.Title,
                    RankingScore = c.RankingScore,
                    PersonalizedScore = c.RankingScore + boost,
                    Tags = c.Tags.Select(t => t.Value).ToList()
                };
            })
            .OrderByDescending(f => f.PersonalizedScore)
            .ThenByDescending(f => f.RankingScore)
            .ToList();

        var total = ranked.Count;
        var items = ranked
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToList();

        return Ok(PagedResult<FeedItem>.Create(items, query.Page, query.PageSize, total));
    }
}
