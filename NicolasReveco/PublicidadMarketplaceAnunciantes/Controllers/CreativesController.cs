using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Gestiona los creativos de una campaña (RF-F3): alta, listado y baja.</summary>
[ApiController]
[Route("api/campaigns/{campaignId:guid}/creatives")]
public class CreativesController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public CreativesController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F3: agrega un creativo a la campaña.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreativeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Add(Guid campaignId, [FromBody] CreateCreativeRequest request)
    {
        var campaignExists = await _db.AdCampaigns.AnyAsync(c => c.Id == campaignId);
        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        if (!Enum.TryParse<CreativeType>(request.Type, ignoreCase: true, out var type))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Tipo de creativo inválido: '{request.Type}'.",
                new { allowed = Enum.GetNames<CreativeType>() }));
        }

        var policyStatusValue = string.IsNullOrWhiteSpace(request.PolicyReviewStatus)
            ? nameof(PolicyReviewStatus.Pending)
            : request.PolicyReviewStatus;

        if (!Enum.TryParse<PolicyReviewStatus>(policyStatusValue, ignoreCase: true, out var policyStatus))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Estado de revisión de políticas inválido: '{request.PolicyReviewStatus}'.",
                new { allowed = Enum.GetNames<PolicyReviewStatus>() }));
        }

        var creative = new Creative
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Type = type,
            AssetUrl = request.AssetUrl,
            Title = request.Title,
            PolicyReviewStatus = policyStatus,
            PolicyReviewNotes = request.PolicyReviewNotes
        };

        _db.Creatives.Add(creative);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(List), new { campaignId }, ToResponse(creative));
    }

    /// <summary>RF-F3: lista los creativos de la campaña (paginado).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CreativeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List(Guid campaignId, [FromQuery] PaginationQuery query)
    {
        var campaignExists = await _db.AdCampaigns.AnyAsync(c => c.Id == campaignId);
        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        var baseQuery = _db.Creatives.Where(c => c.CampaignId == campaignId);
        var totalItems = await baseQuery.CountAsync();

        var creatives = await baseQuery
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = creatives.Select(ToResponse).ToList();
        return Ok(PagedResult<CreativeResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    /// <summary>RF-F3: elimina un creativo de la campaña.</summary>
    [HttpDelete("{creativeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid campaignId, Guid creativeId)
    {
        var creative = await _db.Creatives
            .FirstOrDefaultAsync(c => c.Id == creativeId && c.CampaignId == campaignId);

        if (creative is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Creativo no encontrado."));
        }

        _db.Creatives.Remove(creative);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static CreativeResponse ToResponse(Creative c) => new()
    {
        Id = c.Id,
        CampaignId = c.CampaignId,
        Type = c.Type.ToString(),
        AssetUrl = c.AssetUrl,
        Title = c.Title,
        PolicyReviewStatus = c.PolicyReviewStatus.ToString(),
        PolicyReviewNotes = c.PolicyReviewNotes
    };
}
