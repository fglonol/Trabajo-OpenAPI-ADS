namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Anunciante del marketplace de publicidad (RF-F1).</summary>
public class Advertiser
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string BillingName { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? PaymentMethodToken { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}
