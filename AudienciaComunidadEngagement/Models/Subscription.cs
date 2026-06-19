namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A1: suscripción de un usuario a un canal. Único por (UserId, ChannelId).</summary>
public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChannelId { get; set; }
    public DateTime CreatedAt { get; set; }
}
