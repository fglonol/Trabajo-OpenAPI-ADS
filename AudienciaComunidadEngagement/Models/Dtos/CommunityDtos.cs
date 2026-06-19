namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A6: datos para crear una publicación de comunidad.</summary>
public class CreateCommunityPostDto
{
    public Guid ChannelId { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>RF-A6: representación de respuesta de una publicación de comunidad.</summary>
public class CommunityPostDto
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
