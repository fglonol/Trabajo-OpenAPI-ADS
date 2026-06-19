namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A4: datos para crear una notificación.</summary>
public class CreateNotificationDto
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>RF-A4: representación de respuesta de una notificación.</summary>
public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
