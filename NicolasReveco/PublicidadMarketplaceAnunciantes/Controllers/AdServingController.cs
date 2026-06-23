using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Ad server (RF-F6): decide qué anuncio entregar según placement y targeting.</summary>
[ApiController]
[Route("api/ads")]
public class AdServingController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public AdServingController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// RF-F6: selecciona una campaña Active con presupuesto disponible (Budget &gt; Spend) cuyo
    /// targeting coincida (país e interés contenidos en sus CSV; targeting vacío = match-all) y
    /// devuelve un creativo elegido. Si no hay relleno, responde 404 con código NO_FILL.
    /// </summary>
    [HttpGet("serve")]
    [ProducesResponseType(typeof(AdDecisionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Serve(
        [FromQuery] string? placement,
        [FromQuery] string? country,
        [FromQuery] string? interest)
    {
        // Candidatas: campañas activas con presupuesto disponible y al menos un creativo.
        var candidates = await _db.AdCampaigns
            .Where(c => c.Status == CampaignStatus.Active && c.Budget > c.Spend)
            .Include(c => c.Creatives)
            .Include(c => c.Targeting)
            .OrderByDescending(c => c.Budget - c.Spend)
            .ToListAsync();

        foreach (var campaign in candidates)
        {
            if (campaign.Creatives.Count == 0)
            {
                continue;
            }

            if (!TargetingMatches(campaign.Targeting, country, interest))
            {
                continue;
            }

            var creative = campaign.Creatives.First();
            var decision = new AdDecisionResponse
            {
                CampaignId = campaign.Id,
                CreativeId = creative.Id,
                CreativeType = creative.Type.ToString(),
                AssetUrl = creative.AssetUrl,
                Title = creative.Title
            };

            return Ok(decision);
        }

        return NotFound(new ErrorResponse("NO_FILL",
            "No hay ningún anuncio elegible para los parámetros indicados."));
    }

    /// <summary>
    /// Un targeting nulo o con CSV vacío actúa como match-all. Si define países, el país solicitado
    /// debe estar contenido; si define intereses, el interés solicitado debe estar contenido.
    /// </summary>
    private static bool TargetingMatches(Targeting? targeting, string? country, string? interest)
    {
        if (targeting is null)
        {
            return true;
        }

        if (!CsvContains(targeting.Countries, country))
        {
            return false;
        }

        if (!CsvContains(targeting.Interests, interest))
        {
            return false;
        }

        return true;
    }

    /// <summary>Devuelve true si el CSV está vacío (match-all) o contiene el valor (case-insensitive).</summary>
    private static bool CsvContains(string csv, string? value)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            // El targeting restringe pero no se proporcionó valor para evaluar: no coincide.
            return false;
        }

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(token => string.Equals(token, value, StringComparison.OrdinalIgnoreCase));
    }
}
