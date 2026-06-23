namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Espacio publicitario disponible en el inventario (RF-F5).</summary>
public class AdSlot
{
    public Guid Id { get; set; }
    public Placement Placement { get; set; }
    public Guid? CatalogItemId { get; set; }
    public Guid? ChannelId { get; set; }
    public bool IsContentMonetizable { get; set; }
    public BrandSafetyLevel BrandSafetyLevel { get; set; } = BrandSafetyLevel.Standard;
    public long AvailableImpressions { get; set; }
    public long ReservedImpressions { get; set; }
}
