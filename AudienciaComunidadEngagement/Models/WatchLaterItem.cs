namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A5: item guardado en la lista "ver más tarde" de un usuario.</summary>
public class WatchLaterItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public DateTime AddedAt { get; set; }
}
