namespace AudienciaComunidadEngagement.Models.Dtos;

/// <summary>RF-A5: datos para registrar/actualizar la posición de visualización.</summary>
public class RecordWatchDto
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int PositionSeconds { get; set; }
}

/// <summary>RF-A5: representación de respuesta de una entrada de historial.</summary>
public class WatchHistoryEntryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int PositionSeconds { get; set; }
    public DateTime WatchedAt { get; set; }
}

/// <summary>RF-A5: datos para agregar un item a "ver más tarde".</summary>
public class CreateWatchLaterDto
{
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
}

/// <summary>RF-A5: representación de respuesta de un item "ver más tarde".</summary>
public class WatchLaterItemDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CatalogItemId { get; set; }
    public DateTime AddedAt { get; set; }
}
