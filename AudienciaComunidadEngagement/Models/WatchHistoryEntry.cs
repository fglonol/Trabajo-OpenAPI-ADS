namespace AudienciaComunidadEngagement.Models;

/// <summary>RF-A5: entrada del historial de visualización de un usuario sobre un item.</summary>
public class WatchHistoryEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int PositionSeconds { get; set; }
    public DateTime WatchedAt { get; set; }
}
