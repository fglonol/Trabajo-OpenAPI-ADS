namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>Representación de un contenido indexado devuelta por la API.</summary>
public class IndexedContentResponse
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public double RankingScore { get; set; }
    public double TrendingScore { get; set; }
    public long ImpressionCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}
