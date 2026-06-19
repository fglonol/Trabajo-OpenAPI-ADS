namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>
/// Elemento de tendencias (RF-D4). Trae SOLO referencias y el puntaje de tendencia; la metadata
/// editorial completa debe hidratarse desde Catálogo.
/// </summary>
public class TrendingItem
{
    /// <summary>catalogItemId del contenido (usar para hidratar metadata desde Catálogo).</summary>
    public Guid ContentId { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double TrendingScore { get; set; }
    public long ImpressionCount { get; set; }
    public List<string> Tags { get; set; } = new();
}
