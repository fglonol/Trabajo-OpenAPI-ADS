namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para registrar un evento de medición (RF-F7).</summary>
public class CreateAdEventRequest
{
    public Guid CampaignId { get; set; }
    public Guid CreativeId { get; set; }
    public Guid? CatalogItemId { get; set; }

    /// <summary>Tipo del evento: Impression | Click | Skip.</summary>
    public string Type { get; set; } = string.Empty;
    public decimal CostPerEvent { get; set; }
}

/// <summary>Response de un evento de medición (RF-F7).</summary>
public class AdEventResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid CreativeId { get; set; }
    public Guid? CatalogItemId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal CostPerEvent { get; set; }
    public DateTime OccurredAt { get; set; }
}
