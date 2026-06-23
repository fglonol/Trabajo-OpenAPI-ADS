namespace MonetizacionEcosistemaCreador.Models.Dtos;

/// <summary>Request para evaluar la elegibilidad de un canal (RF-M1).</summary>
public class EvaluateEligibilityRequest
{
    public Guid ChannelId { get; set; }
    public int SubscriberCount { get; set; }
    public double WatchHours { get; set; }
}

/// <summary>Respuesta con el resultado de elegibilidad de un canal (RF-M1).</summary>
public class EligibilityResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public int SubscriberCount { get; set; }
    public double WatchHours { get; set; }
    public bool IsEligible { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? EvaluatedAt { get; set; }
}

public class CreateMonetizationApplicationRequest
{
    public Guid ChannelId { get; set; }
    public Guid CreatorUserId { get; set; }
    public string? Notes { get; set; }
}

public class MonetizationApplicationResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public Guid CreatorUserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class UpdateMonetizationStatusRequest
{
    public bool Enabled { get; set; }
    public string? Reason { get; set; }
}

public class MonetizationStatusResponse
{
    public Guid ResourceId { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Reason { get; set; }
    public DateTime UpdatedAt { get; set; }
}
