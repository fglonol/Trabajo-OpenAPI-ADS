using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonetizacionEcosistemaCreador.Common;
using MonetizacionEcosistemaCreador.Data;
using MonetizacionEcosistemaCreador.Models;
using MonetizacionEcosistemaCreador.Models.Dtos;

namespace MonetizacionEcosistemaCreador.Controllers;

/// <summary>CRUD de productos de monetización por canal (RF-M2).</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly MonetizacionEcosistemaCreadorDbContext _db;

    public ProductsController(MonetizacionEcosistemaCreadorDbContext db)
    {
        _db = db;
    }

    /// <summary>RF-M2: Crea un producto de monetización para un canal.</summary>
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
    {
        if (!Enum.TryParse<ProductType>(request.Type, ignoreCase: true, out var type))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                "El tipo de producto no es válido.", new { allowed = Enum.GetNames<ProductType>() }));
        }

        var entity = new MonetizationProduct
        {
            Id = Guid.NewGuid(),
            ChannelId = request.ChannelId,
            Type = type,
            Name = request.Name,
            Price = request.Price,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(entity));
    }

    /// <summary>RF-M2: Lista paginada de productos de un canal.</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponse>>> List([FromQuery] Guid channelId, [FromQuery] PaginationQuery query)
    {
        var baseQuery = _db.Products.AsNoTracking().Where(p => p.ChannelId == channelId);

        var total = await baseQuery.CountAsync();
        var entities = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        var items = entities.Select(ToResponse).ToList();

        return Ok(PagedResult<ProductResponse>.Create(items, query.Page, query.PageSize, total));
    }

    /// <summary>RF-M2: Obtiene un producto por su Id. 404 si no existe.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
    {
        var entity = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Producto no encontrado."));
        }

        return Ok(ToResponse(entity));
    }

    /// <summary>RF-M2: Actualiza un producto existente. 404 si no existe.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Producto no encontrado."));
        }

        if (!Enum.TryParse<ProductType>(request.Type, ignoreCase: true, out var type))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR",
                "El tipo de producto no es válido.", new { allowed = Enum.GetNames<ProductType>() }));
        }

        entity.Type = type;
        entity.Name = request.Name;
        entity.Price = request.Price;
        entity.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return Ok(ToResponse(entity));
    }

    /// <summary>RF-M2: Elimina un producto. 404 si no existe.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Producto no encontrado."));
        }

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>RF-M2: Registra la compra de una membresía o propina por parte de un espectador y origina el ingreso correspondiente (RF-M3).</summary>
    [HttpPost("purchases")]
    public async Task<ActionResult<ProductPurchaseResponse>> RegisterPurchase([FromBody] CreateProductPurchaseRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null)
        {
            return NotFound(new ErrorResponse("NOT_FOUND", "Producto no encontrado."));
        }

        if (!product.IsActive)
        {
            return Conflict(new ErrorResponse("CONFLICT", "El producto no está activo."));
        }

        var amount = request.Amount > 0 ? request.Amount : product.Price;
        var channelId = request.ChannelId == Guid.Empty ? product.ChannelId : request.ChannelId;
        var now = DateTime.UtcNow;

        // RF-M3: la compra origina un ingreso confirmado (Membership o Tip según el tipo de producto).
        var source = product.Type == ProductType.SuperThanks ? RevenueSource.Tip : RevenueSource.Membership;
        const decimal platformSharePercent = 30m;
        var platformShare = amount * (platformSharePercent / 100m);
        var creatorShare = amount - platformShare;

        var revenueEntry = new RevenueEntry
        {
            Id = Guid.NewGuid(),
            ChannelId = channelId,
            VideoId = request.CatalogItemId,
            Source = source,
            Amount = amount,
            PlatformShare = platformShare,
            CreatorShare = creatorShare,
            Status = RevenueStatus.Confirmed,
            OccurredAt = now
        };
        _db.RevenueEntries.Add(revenueEntry);

        var earnings = await _db.CreatorEarnings.FirstOrDefaultAsync(e => e.ChannelId == channelId);
        if (earnings is null)
        {
            earnings = new CreatorEarnings { Id = Guid.NewGuid(), ChannelId = channelId, Balance = 0m, Currency = "USD" };
            _db.CreatorEarnings.Add(earnings);
        }
        earnings.Balance += creatorShare;
        earnings.UpdatedAt = now;

        var purchase = new ProductPurchase
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            ViewerUserId = request.ViewerUserId,
            ChannelId = channelId,
            CatalogItemId = request.CatalogItemId,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency,
            PurchasedAt = now,
            RevenueEntryId = revenueEntry.Id
        };
        _db.ProductPurchases.Add(purchase);

        await _db.SaveChangesAsync();

        var response = new ProductPurchaseResponse
        {
            Id = purchase.Id,
            ProductId = purchase.ProductId,
            ViewerUserId = purchase.ViewerUserId,
            ChannelId = purchase.ChannelId,
            Amount = purchase.Amount,
            Currency = purchase.Currency,
            PurchasedAt = purchase.PurchasedAt
        };

        return Created($"/api/products/purchases/{response.Id}", response);
    }

    private static ProductResponse ToResponse(MonetizationProduct p) => new()
    {
        Id = p.Id,
        ChannelId = p.ChannelId,
        Type = p.Type.ToString(),
        Name = p.Name,
        Price = p.Price,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt
    };
}
