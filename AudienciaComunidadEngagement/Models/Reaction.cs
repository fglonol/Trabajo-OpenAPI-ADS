namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A2: reacción (like/dislike) de un usuario sobre un item. Única por (UserId, CatalogItemId), actualizable.</summary>
public class Reaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}
