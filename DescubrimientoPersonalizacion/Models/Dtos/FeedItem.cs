namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>
/// Elemento del feed personalizado (RF-D2). Trae SOLO referencias y el puntaje personalizado;
/// la metadata editorial completa debe hidratarse desde Catálogo.
/// </summary>
public class FeedItem
{
    /// <summary>catalogItemId del contenido (usar para hidratar metadata desde Catálogo).</summary>
    public Guid ContentId { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;

    /// <summary>Puntaje base de ranking del contenido.</summary>
    public double RankingScore { get; set; }

    /// <summary>Puntaje personalizado = ranking + boost por intereses del usuario.</summary>
    public double PersonalizedScore { get; set; }
    public List<string> Tags { get; set; } = new();
}
