namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Response del reporte de desempeño de una campaña (RF-F8).</summary>
public class CampaignReportResponse
{
    public Guid CampaignId { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public long Skips { get; set; }
    public double Ctr { get; set; }
    public double Cpm { get; set; }
    public decimal Spend { get; set; }
    public decimal RemainingBudget { get; set; }
    public Guid? InvoiceId { get; set; }
}

/// <summary>Response de una factura de anunciante (RF-F8).</summary>
public class InvoiceResponse
{
    public Guid Id { get; set; }
    public Guid AdvertiserId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public double Cpm { get; set; }
    public decimal TotalSpend { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? IssuedAt { get; set; }
}
