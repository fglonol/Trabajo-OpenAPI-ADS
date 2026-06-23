namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Campaña publicitaria de un anunciante (RF-F2).</summary>
public class AdCampaign
{
    public Guid Id { get; set; }
    public Guid AdvertiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Spend { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Creativos asociados a la campaña (RF-F3).</summary>
    public ICollection<Creative> Creatives { get; set; } = new List<Creative>();

    /// <summary>Targeting (uno-a-uno) de la campaña (RF-F4).</summary>
    public Targeting? Targeting { get; set; }
}
