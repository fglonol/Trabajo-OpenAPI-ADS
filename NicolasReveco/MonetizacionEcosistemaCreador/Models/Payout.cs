namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Pago (payout) solicitado por un canal sobre su saldo disponible (RF-M5).</summary>
public class Payout
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public decimal Amount { get; set; }
    public decimal MinimumPayoutAmount { get; set; } = 100m;
    public Guid? TaxDocumentId { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
