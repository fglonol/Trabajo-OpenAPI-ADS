using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Evaluación y consulta de elegibilidad de monetización de un canal (RF-M1).</summary>
[ApiController]
[Route("api/[controller]")]
public class EligibilityController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public EligibilityController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M1: Evalúa la elegibilidad de un canal (>=1000 suscriptores y >=4000 horas de visualización) y persiste el resultado (upsert por ChannelId).</summary>
    [HttpPost("evaluate")]
    public async Task<ActionResult<EligibilityResponse>> Evaluate([FromBody] EvaluateEligibilityRequest request)
    {
        var isEligible = request.SubscriberCount >= 1000 && request.WatchHours >= 4000;
        var status = isEligible ? EligibilityStatus.Eligible : EligibilityStatus.Rejected;

        var entity = await _db.Eligibilities.FirstOrDefaultAsync(x => x.ChannelId == request.ChannelId);
        if (entity is null)
        {
            entity = new MonetizationEligibility
            {
                Id = Guid.NewGuid(),
                ChannelId = request.ChannelId
            };
            _db.Eligibilities.Add(entity);
        }

        entity.SubscriberCount = request.SubscriberCount;
        entity.WatchHours = request.WatchHours;
        entity.IsEligible = isEligible;
        entity.Status = status;
        entity.EvaluatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToResponse(entity));
    }

    /// <summary>RF-M1: Obtiene el resultado de elegibilidad de un canal. 404 si aún no fue evaluado.</summary>
    [HttpGet("{channelId:guid}")]
    public async Task<ActionResult<EligibilityResponse>> GetByChannel(Guid channelId)
    {
        var entity = await _db.Eligibilities.FirstOrDefaultAsync(x => x.ChannelId == channelId);
        if (entity is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "El canal no tiene una evaluación de elegibilidad."));
        }

        return Ok(ToResponse(entity));
    }

    private static EligibilityResponse ToResponse(MonetizationEligibility e) => new()
    {
        Id = e.Id,
        ChannelId = e.ChannelId,
        SubscriberCount = e.SubscriberCount,
        WatchHours = e.WatchHours,
        IsEligible = e.IsEligible,
        Status = e.Status.ToString(),
        EvaluatedAt = e.EvaluatedAt
    };
}
