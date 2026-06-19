namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A3: datos para crear un comentario.</summary>
public class CreateCommentDto
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Text { get; set; } = string.Empty;
}

/// <summary>RF-A3: datos para editar el texto de un comentario.</summary>
public class UpdateCommentDto
{
    public string Text { get; set; } = string.Empty;
}

/// <summary>RF-A3: representación de respuesta de un comentario.</summary>
public class CommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
}
