namespace PublicidadMarketplaceAnunciantes.Models.Dtos;

/// <summary>Request para definir/reemplazar el targeting de una campaña (RF-F4). Listas como CSV.</summary>
public class SetTargetingRequest
{
    /// <summary>Países objetivo: CSV de códigos ISO-2 (p.ej. "CL,AR,MX").</summary>
    public string Countries { get; set; } = string.Empty;
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }

    /// <summary>Intereses objetivo: CSV (p.ej. "gaming,tech").</summary>
    public string Interests { get; set; } = string.Empty;

    /// <summary>Palabras clave: CSV.</summary>
    public string Keywords { get; set; } = string.Empty;

    /// <summary>Categorías de contenido elegibles: CSV.</summary>
    public string ContentCategories { get; set; } = string.Empty;

    /// <summary>Contenidos excluidos por brand safety: CSV de UUIDs.</summary>
    public string ExcludedCatalogItemIds { get; set; } = string.Empty;
}

/// <summary>Response del targeting de una campaña (RF-F4).</summary>
public class TargetingResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Countries { get; set; } = string.Empty;
    public int AgeMin { get; set; }
    public int AgeMax { get; set; }
    public string Interests { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public string ContentCategories { get; set; } = string.Empty;
    public string ExcludedCatalogItemIds { get; set; } = string.Empty;
}
