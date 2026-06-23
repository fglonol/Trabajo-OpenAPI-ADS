namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Request para solicitar un pago/payout (RF-M5).</summary>
public class CreatePayoutRequest
{
    public Guid ChannelId { get; set; }
    public decimal Amount { get; set; }
    public decimal MinimumPayoutAmount { get; set; } = 100m;
    public Guid? TaxDocumentId { get; set; }
}

/// <summary>Respuesta con los datos de un pago/payout (RF-M5).</summary>
public class PayoutResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public decimal MinimumPayoutAmount { get; set; }
    public Guid? TaxDocumentId { get; set; }
    public DateTime? PaidAt { get; set; }
}
