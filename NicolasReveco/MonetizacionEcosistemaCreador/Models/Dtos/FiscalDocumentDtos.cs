namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Request para registrar documentación fiscal de un canal (RF-M5).</summary>
public class CreateFiscalDocumentRequest
{
    public Guid ChannelId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
}

/// <summary>Respuesta con los datos de un documento fiscal (RF-M5).</summary>
public class FiscalDocumentResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
