namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para crear una campaña (RF-F2).</summary>
public class CreateCampaignRequest
{
    public Guid AdvertiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>Request para actualizar una campaña (RF-F2).</summary>
public class UpdateCampaignRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>Response de una campaña (RF-F2).</summary>
public class CampaignResponse
{
    public Guid Id { get; set; }
    public Guid AdvertiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal Spend { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
