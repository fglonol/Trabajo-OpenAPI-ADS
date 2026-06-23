using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Gestiona las campañas publicitarias (RF-F2): CRUD, transiciones de estado y listado paginado.</summary>
[ApiController]
[Route("api/[controller]")]
public class CampaignsController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public CampaignsController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F2: crea una campaña (Status=Draft).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El nombre de la campaña es obligatorio."));
        }

        var advertiserExists = await _db.Advertisers.AnyAsync(a => a.Id == request.AdvertiserId);
        if (!advertiserExists)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Anunciante no encontrado."));
        }

        var campaign = new AdCampaign
        {
            Id = Guid.NewGuid(),
            AdvertiserId = request.AdvertiserId,
            Name = request.Name,
            Budget = request.Budget,
            Spend = 0m,
            Status = CampaignStatus.Draft,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.AdCampaigns.Add(campaign);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, ToResponse(campaign));
    }

    /// <summary>RF-F2: listado paginado de campañas, con filtros opcionales por anunciante y estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CampaignResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? advertiserId,
        [FromQuery] string? status)
    {
        var baseQuery = _db.AdCampaigns.AsQueryable();

        if (advertiserId.HasValue)
        {
            baseQuery = baseQuery.Where(c => c.AdvertiserId == advertiserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<CampaignStatus>(status, ignoreCase: true, out var parsed))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    $"Estado inválido: '{status}'.",
                    new { allowed = Enum.GetNames<CampaignStatus>() }));
            }

            baseQuery = baseQuery.Where(c => c.Status == parsed);
        }

        var totalItems = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToResponse).ToList();
        return Ok(PagedResult<CampaignResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    /// <summary>RF-F2: obtiene una campaña por su Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        return Ok(ToResponse(campaign));
    }

    /// <summary>RF-F2: actualiza una campaña.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCampaignRequest request)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        campaign.Name = request.Name;
        campaign.Budget = request.Budget;
        campaign.StartDate = request.StartDate;
        campaign.EndDate = request.EndDate;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(campaign));
    }

    /// <summary>RF-F2: elimina una campaña (cascada de creativos y targeting).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        _db.AdCampaigns.Remove(campaign);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>RF-F2: activa una campaña (Draft -> Active).</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        if (campaign.Status != CampaignStatus.Draft)
        {
            return Conflict(new ErrorResponse("CONFLICT",
                "Solo se puede activar una campaña en estado Draft."));
        }

        campaign.Status = CampaignStatus.Active;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(campaign));
    }

    /// <summary>RF-F2: reanuda una campaña pausada (Paused -> Active) si tiene presupuesto disponible.</summary>
    [HttpPost("{id:guid}/resume")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Resume(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        if (campaign.Status != CampaignStatus.Paused)
        {
            return Conflict(new ErrorResponse("CONFLICT",
                "Solo se puede reanudar una campaña en estado Paused."));
        }

        if (campaign.Budget <= campaign.Spend)
        {
            return Conflict(new ErrorResponse("CONFLICT",
                "La campaña no tiene presupuesto disponible para reanudar."));
        }

        campaign.Status = CampaignStatus.Active;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(campaign));
    }

    /// <summary>RF-F2: pausa una campaña (-> Paused).</summary>
    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pause(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        campaign.Status = CampaignStatus.Paused;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(campaign));
    }

    /// <summary>RF-F2: completa una campaña (-> Completed).</summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(CampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var campaign = await _db.AdCampaigns.FindAsync(id);
        if (campaign is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Campaña no encontrada."));
        }

        campaign.Status = CampaignStatus.Completed;
        await _db.SaveChangesAsync();
        return Ok(ToResponse(campaign));
    }

    private static CampaignResponse ToResponse(AdCampaign c) => new()
    {
        Id = c.Id,
        AdvertiserId = c.AdvertiserId,
        Name = c.Name,
        Budget = c.Budget,
        Spend = c.Spend,
        Status = c.Status.ToString(),
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        CreatedAt = c.CreatedAt
    };
}
