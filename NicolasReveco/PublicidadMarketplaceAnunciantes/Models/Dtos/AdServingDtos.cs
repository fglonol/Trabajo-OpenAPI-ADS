namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Decisión de entrega de anuncio devuelta por el ad server (RF-F6).</summary>
public class AdDecisionResponse
{
    public Guid CampaignId { get; set; }
    public Guid CreativeId { get; set; }
    public string CreativeType { get; set; } = string.Empty;
    public string AssetUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
