namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Desglose de ingresos por origen (RF-M4).</summary>
public class RevenueBreakdownItem
{
    public string Source { get; set; } = string.Empty;
    public Guid? VideoId { get; set; }
    public string? Period { get; set; }
    public decimal Total { get; set; }
}

/// <summary>Respuesta de ganancias de un canal con su saldo y desglose por origen (RF-M4).</summary>
public class EarningsResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid CreatorUserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime UpdatedAt { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public double Rpm { get; set; }
    public string? ExportUrl { get; set; }
    public IReadOnlyCollection<RevenueBreakdownItem> Breakdown { get; set; } = new List<RevenueBreakdownItem>();
}
