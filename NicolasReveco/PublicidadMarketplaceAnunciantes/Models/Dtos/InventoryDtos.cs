namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para crear un espacio publicitario de inventario (RF-F5).</summary>
public class CreateAdSlotRequest
{
    /// <summary>Ubicación: PreRoll | MidRoll | PostRoll | Banner.</summary>
    public string Placement { get; set; } = string.Empty;
    public Guid? CatalogItemId { get; set; }
    public Guid? ChannelId { get; set; }
    public bool IsContentMonetizable { get; set; }
    /// <summary>Nivel de brand safety: Standard | Limited | Restricted.</summary>
    public string BrandSafetyLevel { get; set; } = "Standard";
    public long AvailableImpressions { get; set; }
    public long ReservedImpressions { get; set; }
}

/// <summary>Request para actualizar un espacio publicitario de inventario (RF-F5).</summary>
public class UpdateAdSlotRequest
{
    public string Placement { get; set; } = string.Empty;
    public Guid? CatalogItemId { get; set; }
    public Guid? ChannelId { get; set; }
    public bool IsContentMonetizable { get; set; }
    public string BrandSafetyLevel { get; set; } = "Standard";
    public long AvailableImpressions { get; set; }
    public long ReservedImpressions { get; set; }
}

/// <summary>Response de un espacio publicitario de inventario (RF-F5).</summary>
public class AdSlotResponse
{
    public Guid Id { get; set; }
    public string Placement { get; set; } = string.Empty;
    public Guid? CatalogItemId { get; set; }
    public Guid? ChannelId { get; set; }
    public bool IsContentMonetizable { get; set; }
    public string BrandSafetyLevel { get; set; } = string.Empty;
    public long AvailableImpressions { get; set; }
    public long ReservedImpressions { get; set; }
}

/// <summary>Criterios usados para estimar inventario antes de activar una campaña (RF-F4/RF-F5).</summary>
public class EstimateInventoryRequest
{
    public string? Placement { get; set; }
    /// <summary>CSV de códigos ISO-2.</summary>
    public string? Countries { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public string? Interests { get; set; }
    public string? ContentCategories { get; set; }
    public string? BrandSafetyLevel { get; set; }
}

/// <summary>Resultado de la estimación de inventario disponible (RF-F4/RF-F5).</summary>
public class InventoryEstimateResponse
{
    public long EstimatedImpressions { get; set; }
    public long EligibleCatalogItems { get; set; }
    public double EstimatedCpm { get; set; }
    public string? Notes { get; set; }
}
