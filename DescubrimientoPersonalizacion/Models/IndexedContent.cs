namespace DescubrimientoPersonalizacion.Models;

/// <summary>
/// Referencia indexada de un contenido publicado (RF-D6). El <see cref="Id"/> es IGUAL al
/// catalogItemId del bounded context Catálogo. Guarda SOLO referencias y puntajes de ranking,
/// no la metadata editorial completa (que vive en Catálogo); el <see cref="Title"/> es una copia
/// ligera para soportar la búsqueda.
/// </summary>
public class IndexedContent
{
    /// <summary>Identificador del contenido. IGUAL al catalogItemId de Catálogo.</summary>
    public Guid Id { get; set; }

    /// <summary>Canal propietario del contenido.</summary>
    public Guid ChannelId { get; set; }

    /// <summary>Copia ligera del título para búsqueda (no es la metadata editorial completa).</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Puntaje de ranking acumulado por las señales de comportamiento.</summary>
    public double RankingScore { get; set; }

    /// <summary>Puntaje de tendencia acumulado por las señales recientes.</summary>
    public double TrendingScore { get; set; }

    /// <summary>Número de impresiones registradas.</summary>
    public long ImpressionCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>Etiquetas asociadas para búsqueda y relacionados.</summary>
    public ICollection<ContentTag> Tags { get; set; } = new List<ContentTag>();
}
