namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>
/// Factura de un anunciante por un periodo (RF-F8). No existe en el OpenAPI ningún endpoint de
/// creación: este bounded context solo expone lectura (GET /api/reporting/invoices/{advertiserId}).
/// La generación de invoices (p.ej. un job periódico de facturación) está fuera del alcance de los
/// archivos provistos.
/// </summary>
public class Invoice
{
    public Guid Id { get; set; }
    public Guid AdvertiserId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public double Cpm { get; set; }
    public decimal TotalSpend { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime? IssuedAt { get; set; }
}
