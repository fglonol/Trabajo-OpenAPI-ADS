using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Gestiona el inventario de espacios publicitarios (RF-F5): CRUD y listado paginado.</summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public InventoryController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F5: crea un espacio publicitario de inventario.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AdSlotResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAdSlotRequest request)
    {
        if (!Enum.TryParse<Placement>(request.Placement, ignoreCase: true, out var placement))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Placement inválido: '{request.Placement}'.",
                new { allowed = Enum.GetNames<Placement>() }));
        }

        var brandSafetyValue = string.IsNullOrWhiteSpace(request.BrandSafetyLevel)
            ? nameof(BrandSafetyLevel.Standard)
            : request.BrandSafetyLevel;

        if (!Enum.TryParse<BrandSafetyLevel>(brandSafetyValue, ignoreCase: true, out var brandSafety))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Nivel de brand safety inválido: '{request.BrandSafetyLevel}'.",
                new { allowed = Enum.GetNames<BrandSafetyLevel>() }));
        }

        var slot = new AdSlot
        {
            Id = Guid.NewGuid(),
            Placement = placement,
            CatalogItemId = request.CatalogItemId,
            ChannelId = request.ChannelId,
            IsContentMonetizable = request.IsContentMonetizable,
            BrandSafetyLevel = brandSafety,
            AvailableImpressions = request.AvailableImpressions,
            ReservedImpressions = request.ReservedImpressions
        };

        _db.AdSlots.Add(slot);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = slot.Id }, ToResponse(slot));
    }

    /// <summary>
    /// RF-F4/RF-F5: estima el inventario disponible para unos criterios de targeting, a partir de
    /// los espacios monetizables que coinciden en placement y brand safety. No modela CPM histórico
    /// (RF-F8 no expone esa fuente en este bounded context), por lo que estimatedCpm es un valor de
    /// referencia fijo y se documenta en notes.
    /// </summary>
    [HttpPost("estimate")]
    [ProducesResponseType(typeof(InventoryEstimateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Estimate([FromBody] EstimateInventoryRequest request)
    {
        Placement? placement = null;
        if (!string.IsNullOrWhiteSpace(request.Placement))
        {
            if (!Enum.TryParse<Placement>(request.Placement, ignoreCase: true, out var parsedPlacement))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    $"Placement inválido: '{request.Placement}'.",
                    new { allowed = Enum.GetNames<Placement>() }));
            }

            placement = parsedPlacement;
        }

        BrandSafetyLevel? brandSafety = null;
        if (!string.IsNullOrWhiteSpace(request.BrandSafetyLevel))
        {
            if (!Enum.TryParse<BrandSafetyLevel>(request.BrandSafetyLevel, ignoreCase: true, out var parsedBrandSafety))
            {
                return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                    $"Nivel de brand safety inválido: '{request.BrandSafetyLevel}'.",
                    new { allowed = Enum.GetNames<BrandSafetyLevel>() }));
            }

            brandSafety = parsedBrandSafety;
        }

        var query = _db.AdSlots.Where(s => s.IsContentMonetizable);

        if (placement.HasValue)
        {
            query = query.Where(s => s.Placement == placement.Value);
        }

        if (brandSafety.HasValue)
        {
            query = query.Where(s => s.BrandSafetyLevel == brandSafety.Value);
        }

        var matches = await query.ToListAsync();

        var estimatedImpressions = matches.Sum(s => Math.Max(0L, s.AvailableImpressions - s.ReservedImpressions));
        var eligibleCatalogItems = matches
            .Where(s => s.CatalogItemId.HasValue)
            .Select(s => s.CatalogItemId!.Value)
            .Distinct()
            .LongCount();

        var response = new InventoryEstimateResponse
        {
            EstimatedImpressions = estimatedImpressions,
            EligibleCatalogItems = eligibleCatalogItems,
            EstimatedCpm = 0d,
            Notes = "Estimación basada en el snapshot actual de inventario monetizable que coincide con " +
                     "placement y brandSafetyLevel. No se modela CPM histórico en este bounded context; " +
                     "estimatedCpm se devuelve en 0 hasta integrar esa fuente."
        };

        return Ok(response);
    }

    /// <summary>RF-F5: listado paginado de espacios publicitarios.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdSlotResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.AdSlots.AsQueryable();
        var totalItems = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderBy(s => s.Placement)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToResponse).ToList();
        return Ok(PagedResult<AdSlotResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    /// <summary>RF-F5: obtiene un espacio publicitario por su Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var slot = await _db.AdSlots.FindAsync(id);
        if (slot is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Espacio publicitario no encontrado."));
        }

        return Ok(ToResponse(slot));
    }

    /// <summary>RF-F5: actualiza un espacio publicitario.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdSlotRequest request)
    {
        var slot = await _db.AdSlots.FindAsync(id);
        if (slot is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Espacio publicitario no encontrado."));
        }

        if (!Enum.TryParse<Placement>(request.Placement, ignoreCase: true, out var placement))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Placement inválido: '{request.Placement}'.",
                new { allowed = Enum.GetNames<Placement>() }));
        }

        var brandSafetyValue = string.IsNullOrWhiteSpace(request.BrandSafetyLevel)
            ? nameof(BrandSafetyLevel.Standard)
            : request.BrandSafetyLevel;

        if (!Enum.TryParse<BrandSafetyLevel>(brandSafetyValue, ignoreCase: true, out var brandSafety))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                $"Nivel de brand safety inválido: '{request.BrandSafetyLevel}'.",
                new { allowed = Enum.GetNames<BrandSafetyLevel>() }));
        }

        slot.Placement = placement;
        slot.CatalogItemId = request.CatalogItemId;
        slot.ChannelId = request.ChannelId;
        slot.IsContentMonetizable = request.IsContentMonetizable;
        slot.BrandSafetyLevel = brandSafety;
        slot.AvailableImpressions = request.AvailableImpressions;
        slot.ReservedImpressions = request.ReservedImpressions;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(slot));
    }

    /// <summary>RF-F5: elimina un espacio publicitario.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var slot = await _db.AdSlots.FindAsync(id);
        if (slot is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Espacio publicitario no encontrado."));
        }

        _db.AdSlots.Remove(slot);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static AdSlotResponse ToResponse(AdSlot s) => new()
    {
        Id = s.Id,
        Placement = s.Placement.ToString(),
        CatalogItemId = s.CatalogItemId,
        ChannelId = s.ChannelId,
        IsContentMonetizable = s.IsContentMonetizable,
        BrandSafetyLevel = s.BrandSafetyLevel.ToString(),
        AvailableImpressions = s.AvailableImpressions,
        ReservedImpressions = s.ReservedImpressions
    };
}
