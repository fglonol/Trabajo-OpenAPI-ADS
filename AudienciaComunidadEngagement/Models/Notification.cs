namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A4: notificación dirigida a un usuario (nuevo comentario, reacción, etc.).</summary>
public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
