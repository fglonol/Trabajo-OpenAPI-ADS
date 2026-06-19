namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A1: datos para crear una suscripción.</summary>
public class CreateSubscriptionDto
{
    public Guid UserId { get; set; }
    public Guid ChannelId { get; set; }
}

/// <summary>RF-A1: representación de respuesta de una suscripción.</summary>
public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChannelId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>RF-A1: cantidad de suscriptores de un canal (consumido por Monetización).</summary>
public class SubscriberCountDto
{
    public Guid ChannelId { get; set; }
    public int Count { get; set; }
}
