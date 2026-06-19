using DescubrimientoPersonalizacion.Common;
using DescubrimientoPersonalizacion.Data;
using DescubrimientoPersonalizacion.Models;
using DescubrimientoPersonalizacion.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DescubrimientoPersonalizacion.Controllers;

/// <summary>RF-D5 Captura de señales: registra señales de comportamiento y actualiza ranking/intereses.</summary>
[ApiController]
[Route("api/[controller]")]
public class SignalsController : ControllerBase
{
    private readonly DescubrimientoPersonalizacionDbContext _db;

    public SignalsController(DescubrimientoPersonalizacionDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-D5: Registra una señal de comportamiento. Efectos secundarios sobre el contenido objetivo:
    /// suma Weight a RankingScore y TrendingScore, incrementa ImpressionCount en 1 y hace upsert de los
    /// UserInterest del usuario a partir de las etiquetas del contenido (sumando Weight a su Score).
    /// 404 si el contenido objetivo no está indexado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Signal), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Signal>> Capture([FromBody] SignalRequest request)
    {
        if (request.UserId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El 'userId' es obligatorio."));
        if (request.ContentId == Guid.Empty)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El 'contentId' es obligatorio."));

        var content = await _db.IndexedContents
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == request.ContentId);

        if (content is null)
            return NotFound(new ErrorResponse("NOT_FOUND", "Contenido no encontrado en el índice."));

        // Peso por defecto si viene 0 o negativo (según contrato del DTO).
        var weight = request.Weight <= 0 ? 1 : request.Weight;
        var now = DateTime.UtcNow;

        var signal = new Signal
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            ContentId = request.ContentId,
            Type = request.Type,
            Weight = weight,
            CreatedAt = now
        };
        _db.Signals.Add(signal);

        // Efectos secundarios sobre el contenido objetivo.
        content.RankingScore += weight;
        content.TrendingScore += weight;
        content.ImpressionCount += 1;
        content.UpdatedAt = now;

        // Upsert de los intereses del usuario a partir de las etiquetas del contenido.
        var tagValues = content.Tags.Select(t => t.Value).ToList();
        if (tagValues.Count > 0)
        {
            var existingInterests = await _db.UserInterests
                .Where(i => i.UserId == request.UserId && tagValues.Contains(i.Interest))
                .ToListAsync();

            foreach (var tag in tagValues)
            {
                var interest = existingInterests
                    .FirstOrDefault(i => string.Equals(i.Interest, tag, StringComparison.OrdinalIgnoreCase));

                if (interest is null)
                {
                    _db.UserInterests.Add(new UserInterest
                    {
                        Id = Guid.NewGuid(),
                        UserId = request.UserId,
                        Interest = tag,
                        Score = weight,
                        UpdatedAt = now
                    });
                }
                else
                {
                    interest.Score += weight;
                    interest.UpdatedAt = now;
                }
            }
        }

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Capture), new { id = signal.Id }, signal);
    }
}
