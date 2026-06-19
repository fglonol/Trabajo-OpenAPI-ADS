namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A6: publicación de comunidad de un canal.</summary>
public class CommunityPost
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
