namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Creativo publicitario hijo de una campaña (RF-F3).</summary>
public class Creative
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public CreativeType Type { get; set; }
    public string AssetUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PolicyReviewStatus PolicyReviewStatus { get; set; } = PolicyReviewStatus.Pending;
    public string? PolicyReviewNotes { get; set; }
}
