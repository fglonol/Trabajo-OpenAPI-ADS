namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Saldo de ganancias de un creador/canal. Un registro por canal (RF-M4).</summary>
public class CreatorEarnings
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid CreatorUserId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime UpdatedAt { get; set; }
}
