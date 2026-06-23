namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Solicitud explícita de un creador para monetizar su canal (RF-M1).</summary>
public class MonetizationApplication
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid CreatorUserId { get; set; }
    public string? Notes { get; set; }
    public EligibilityStatus Status { get; set; } = EligibilityStatus.UnderReview;
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
