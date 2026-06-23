namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para crear un anunciante (RF-F1).</summary>
public class CreateAdvertiserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PaymentMethodToken { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>Request para actualizar un anunciante (RF-F1).</summary>
public class UpdateAdvertiserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PaymentMethodToken { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>Response de un anunciante (RF-F1).</summary>
public class AdvertiserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}
