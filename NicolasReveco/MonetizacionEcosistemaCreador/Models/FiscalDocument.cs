namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Documentación fiscal requerida para poder cobrar (RF-M5).</summary>
public class FiscalDocument
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public FiscalDocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public FiscalDocumentStatus Status { get; set; } = FiscalDocumentStatus.PendingReview;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
