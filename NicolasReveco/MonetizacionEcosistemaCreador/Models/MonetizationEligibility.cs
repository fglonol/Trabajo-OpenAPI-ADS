namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Resultado de la evaluación de elegibilidad de monetización de un canal (RF-M1).</summary>
public class MonetizationEligibility
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public int SubscriberCount { get; set; }
    public double WatchHours { get; set; }
    public bool IsEligible { get; set; }
    public EligibilityStatus Status { get; set; } = EligibilityStatus.NotEvaluated;
    public DateTime? EvaluatedAt { get; set; }
}
