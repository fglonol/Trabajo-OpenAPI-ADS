namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para agregar un creativo a una campaña (RF-F3).</summary>
public class CreateCreativeRequest
{
    /// <summary>Tipo del creativo: VideoAd | Banner.</summary>
    public string Type { get; set; } = string.Empty;
    public string AssetUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    /// <summary>Estado de revisión de políticas: Pending | Approved | Rejected. Default Pending.</summary>
    public string PolicyReviewStatus { get; set; } = "Pending";
    public string? PolicyReviewNotes { get; set; }
}

/// <summary>Response de un creativo (RF-F3).</summary>
public class CreativeResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string AssetUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PolicyReviewStatus { get; set; } = string.Empty;
    public string? PolicyReviewNotes { get; set; }
}
