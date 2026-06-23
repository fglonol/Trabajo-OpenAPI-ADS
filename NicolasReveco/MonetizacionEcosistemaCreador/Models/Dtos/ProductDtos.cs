namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Request para crear un producto de monetización (RF-M2).</summary>
public class CreateProductRequest
{
    public Guid ChannelId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>Request para actualizar un producto de monetización (RF-M2).</summary>
public class UpdateProductRequest
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Respuesta con los datos de un producto de monetización (RF-M2).</summary>
public class ProductResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductPurchaseRequest
{
    public Guid ProductId { get; set; }
    public Guid ViewerUserId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? CatalogItemId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

public class ProductPurchaseResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ViewerUserId { get; set; }
    public Guid ChannelId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PurchasedAt { get; set; }
}
