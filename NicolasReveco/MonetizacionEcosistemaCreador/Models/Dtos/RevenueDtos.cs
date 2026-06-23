namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Request para registrar un ingreso de un canal (RF-M3).</summary>
public class CreateRevenueRequest
{
    public Guid ChannelId { get; set; }
    public Guid? VideoId { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Confirmed";
    public decimal PlatformSharePercent { get; set; } = 30m;
    public DateTime? OccurredAt { get; set; }
}

/// <summary>Respuesta con los datos de un ingreso registrado (RF-M3).</summary>
public class RevenueResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? VideoId { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PlatformShare { get; set; }
    public decimal CreatorShare { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
