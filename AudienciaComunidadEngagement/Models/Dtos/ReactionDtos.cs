namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A2: datos para crear/actualizar (upsert) una reacción.</summary>
public class CreateReactionDto
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public ReactionType Type { get; set; }
}

/// <summary>RF-A2: representación de respuesta de una reacción.</summary>
public class ReactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>RF-A2: resumen de reacciones de un item (likes/dislikes).</summary>
public class ReactionSummaryDto
{
    public Guid CatalogItemId { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}
