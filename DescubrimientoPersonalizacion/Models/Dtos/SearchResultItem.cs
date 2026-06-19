namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>
/// Resultado de búsqueda / relacionados (RF-D1, RF-D3). Trae SOLO referencias y puntajes; la
/// metadata editorial completa debe hidratarse desde Catálogo (GET /api/catalogitems/{id}).
/// </summary>
public class SearchResultItem
{
    /// <summary>catalogItemId del contenido (usar para hidratar metadata desde Catálogo).</summary>
    public Guid ContentId { get; set; }
    public Guid ChannelId { get; set; }

    /// <summary>Copia ligera del título indexado (no es la metadata editorial completa).</summary>
    public string Title { get; set; } = string.Empty;
    public double RankingScore { get; set; }
    public List<string> Tags { get; set; } = new();
}
