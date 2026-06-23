using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>Solicitud de monetización y activación/desactivación a nivel canal o contenido (RF-M1).</summary>
[ApiController]
[Route("api/monetization")]
public class MonetizationController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public MonetizationController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M1: Permite que un creador solicite monetizar su canal. 409 si ya existe una solicitud pendiente.</summary>
    [HttpPost("applications")]
    public async Task<ActionResult<MonetizationApplicationResponse>> CreateApplication(
        [FromBody] CreateMonetizationApplicationRequest request)
    {
        var pending = await _db.MonetizationApplications.FirstOrDefaultAsync(
            a => a.ChannelId == request.ChannelId && a.Status == EligibilityStatus.UnderReview);
        if (pending is not null)
        {
            return Conflict(new ErrorResponse("CONFLICT", "Ya existe una solicitud de monetización pendiente para este canal."));
        }

        var entity = new MonetizationApplication
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            CreatorUserId = request.CreatorUserId,
            Notes = request.Notes,
            Status = EligibilityStatus.UnderReview,
            RequestedAt = DateTime.UtcNow
        };

        _db.MonetizationApplications.Add(entity);
        await _db.SaveChangesAsync();

        return Created($"/api/monetization/applications/{entity.Id}", ToResponse(entity));
    }

    /// <summary>RF-M1: Activa o desactiva la monetización a nivel de canal (upsert por ResourceId).</summary>
    [HttpPut("channels/{channelId:guid}/status")]
    public Task<ActionResult<MonetizationStatusResponse>> UpdateChannelStatus(
        Guid channelId, [FromBody] UpdateMonetizationStatusRequest request)
        => UpsertStatus(channelId, MonetizationResourceType.Channel, request);

    /// <summary>RF-M1: Activa o desactiva la monetización para un contenido específico (upsert por ResourceId).</summary>
    [HttpPut("content/{catalogItemId:guid}/status")]
    public Task<ActionResult<MonetizationStatusResponse>> UpdateContentStatus(
        Guid catalogItemId, [FromBody] UpdateMonetizationStatusRequest request)
        => UpsertStatus(catalogItemId, MonetizationResourceType.Content, request);

    private async Task<ActionResult<MonetizationStatusResponse>> UpsertStatus(
        Guid resourceId, MonetizationResourceType resourceType, UpdateMonetizationStatusRequest request)
    {
        var entity = await _db.MonetizationStatuses.FirstOrDefaultAsync(
            s => s.ResourceId == resourceId && s.ResourceType == resourceType);

        if (entity is null)
        {
            entity = new MonetizationStatus
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                ResourceType = resourceType
            };
            _db.MonetizationStatuses.Add(entity);
        }

        entity.Enabled = request.Enabled;
        entity.Reason = request.Reason;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new MonetizationStatusResponse
        {
            ResourceId = entity.ResourceId,
            ResourceType = entity.ResourceType.ToString(),
            Enabled = entity.Enabled,
            Reason = entity.Reason,
            UpdatedAt = entity.UpdatedAt
        });
    }

    private static MonetizationApplicationResponse ToResponse(MonetizationApplication a) => new()
    {
        Id = a.Id,
        ChannelId = a.ChannelId,
        CreatorUserId = a.CreatorUserId,
        Status = a.Status.ToString(),
        RequestedAt = a.RequestedAt,
        ReviewedAt = a.ReviewedAt
    };
}
