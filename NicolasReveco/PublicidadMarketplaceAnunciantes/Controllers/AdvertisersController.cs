using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicidadMarketplaceAnunciantes.Common;
using PublicidadMarketplaceAnunciantes.Data;
using PublicidadMarketplaceAnunciantes.Models;
using PublicidadMarketplaceAnunciantes.Models.Dtos;

namespace PublicidadMarketplaceAnunciantes.Controllers;

/// <summary>Gestiona los anunciantes del marketplace (RF-F1): CRUD y listado paginado.</summary>
[ApiController]
[Route("api/[controller]")]
public class AdvertisersController : ControllerBase
{
    private readonly PublicidadMarketplaceAnunciantesDbContext _db;

    public AdvertisersController(PublicidadMarketplaceAnunciantesDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-F1: crea un anunciante.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AdvertiserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAdvertiserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "El nombre y el email son obligatorios."));
        }

        var advertiser = new Advertiser
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            BillingName = request.BillingName,
            BillingAddress = request.BillingAddress,
            TaxId = request.TaxId,
            PaymentMethodToken = request.PaymentMethodToken,
            Balance = request.Balance,
            CreatedAt = DateTime.UtcNow
        };

        _db.Advertisers.Add(advertiser);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = advertiser.Id }, ToResponse(advertiser));
    }

    /// <summary>RF-F1: listado paginado de anunciantes.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdvertiserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.Advertisers.AsQueryable();
        var totalItems = await baseQuery.CountAsync();

        var items = await baseQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var dtos = items.Select(ToResponse).ToList();
        return Ok(PagedResult<AdvertiserResponse>.Create(dtos, query.Page, query.PageSize, totalItems));
    }

    /// <summary>RF-F1: obtiene un anunciante por su Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdvertiserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var advertiser = await _db.Advertisers.FindAsync(id);
        if (advertiser is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Anunciante no encontrado."));
        }

        return Ok(ToResponse(advertiser));
    }

    /// <summary>RF-F1: actualiza un anunciante.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AdvertiserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdvertiserRequest request)
    {
        var advertiser = await _db.Advertisers.FindAsync(id);
        if (advertiser is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Anunciante no encontrado."));
        }

        advertiser.Name = request.Name;
        advertiser.Email = request.Email;
        advertiser.BillingName = request.BillingName;
        advertiser.BillingAddress = request.BillingAddress;
        advertiser.TaxId = request.TaxId;
        advertiser.PaymentMethodToken = request.PaymentMethodToken;
        advertiser.Balance = request.Balance;

        await _db.SaveChangesAsync();
        return Ok(ToResponse(advertiser));
    }

    /// <summary>RF-F1: elimina un anunciante.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var advertiser = await _db.Advertisers.FindAsync(id);
        if (advertiser is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Anunciante no encontrado."));
        }

        _db.Advertisers.Remove(advertiser);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static AdvertiserResponse ToResponse(Advertiser a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Email = a.Email,
        BillingName = a.BillingName,
        BillingAddress = a.BillingAddress,
        TaxId = a.TaxId,
        Balance = a.Balance,
        CreatedAt = a.CreatedAt
    };
}
