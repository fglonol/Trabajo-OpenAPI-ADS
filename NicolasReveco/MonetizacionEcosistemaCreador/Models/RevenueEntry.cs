namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Registro de un ingreso percibido por un canal. Incrementa el saldo de ganancias (RF-M3).</summary>
public class RevenueEntry
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid? VideoId { get; set; }
    public RevenueSource Source { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformShare { get; set; }
    public decimal CreatorShare { get; set; }
    public RevenueStatus Status { get; set; } = RevenueStatus.Pending;
    public DateTime OccurredAt { get; set; }
}
