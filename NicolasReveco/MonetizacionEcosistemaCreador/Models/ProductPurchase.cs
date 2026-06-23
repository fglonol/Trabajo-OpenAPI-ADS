namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Compra de una membresía o propina por parte de un espectador (RF-M2).</summary>
public class ProductPurchase
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ViewerUserId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? CatalogItemId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PurchasedAt { get; set; }
    public Guid? RevenueEntryId { get; set; }
}
