namespace DescubrimientoPersonalizacion.Models;

/// <summary>Etiqueta hija de <see cref="IndexedContent"/> usada para búsqueda y relacionados.</summary>
public class ContentTag
{
    public Guid Id { get; set; }

    /// <summary>FK hacia el contenido indexado dueño de la etiqueta.</summary>
    public Guid IndexedContentId { get; set; }

    /// <summary>Valor de la etiqueta (p.ej. "musica", "tutorial").</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Navegación al contenido indexado padre.</summary>
    public IndexedContent? IndexedContent { get; set; }
}
