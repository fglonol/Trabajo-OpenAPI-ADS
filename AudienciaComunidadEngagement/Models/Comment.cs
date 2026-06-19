namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A3: comentario de un usuario sobre un item del catálogo, con respuestas anidadas opcionales.</summary>
public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
}
