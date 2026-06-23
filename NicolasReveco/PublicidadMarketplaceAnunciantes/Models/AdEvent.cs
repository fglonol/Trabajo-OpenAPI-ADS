namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Evento de medición publicitaria: impresión o clic (RF-F7).</summary>
public class AdEvent
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid CreativeId { get; set; }
    public Guid? CatalogItemId { get; set; }
    public AdEventType Type { get; set; }
    public decimal CostPerEvent { get; set; }
    public DateTime OccurredAt { get; set; }
}
