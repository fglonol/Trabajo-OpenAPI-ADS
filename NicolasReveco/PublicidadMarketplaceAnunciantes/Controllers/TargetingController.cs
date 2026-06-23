using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Gestiona el targeting (uno-a-uno) de una campaña (RF-F4): definir/reemplazar y consultar.</summary>
[ApiController]
[Route("api/campaigns/{campaignId:guid}/targeting")]
public class TargetingController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public TargetingController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F4: define o reemplaza el targeting de la campaña.</summary>
    [HttpPut]
    [ProducesResponseType(typeof(TargetingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Set(Guid campaignId, [FromBody] SetTargetingRequest request)
    {
        var campaignExists = await _db.AdCampaigns.AnyAsync(c => c.Id == campaignId);
        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        var targeting = await _db.Targetings.FirstOrDefaultAsync(t => t.CampaignId == campaignId);
        if (targeting is null)
        {
            targeting = new Targeting
            {
                Id = Guid.NewGuid(),
                CampaignId = campaignId
            };
            _db.Targetings.Add(targeting);
        }

        targeting.Countries = request.Countries;
        targeting.AgeMin = request.AgeMin;
        targeting.AgeMax = request.AgeMax;
        targeting.Interests = request.Interests;
        targeting.Keywords = request.Keywords;
        targeting.ContentCategories = request.ContentCategories;
        targeting.ExcludedCatalogItemIds = request.ExcludedCatalogItemIds;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(targeting));
    }

    /// <summary>RF-F4: obtiene el targeting de la campaña.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TargetingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId)
    {
        var campaignExists = await _db.AdCampaigns.AnyAsync(c => c.Id == campaignId);
        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        var targeting = await _db.Targetings.FirstOrDefaultAsync(t => t.CampaignId == campaignId);
        if (targeting is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "La campaña no tiene targeting definido."));
        }

        return Ok(ToResponse(targeting));
    }

    private static TargetingResponse ToResponse(Targeting t) => new()
    {
        Id = t.Id,
        CampaignId = t.CampaignId,
        Countries = t.Countries,
        AgeMin = t.AgeMin,
        AgeMax = t.AgeMax,
        Interests = t.Interests,
        Keywords = t.Keywords,
        ContentCategories = t.ContentCategories,
        ExcludedCatalogItemIds = t.ExcludedCatalogItemIds
    };
}
