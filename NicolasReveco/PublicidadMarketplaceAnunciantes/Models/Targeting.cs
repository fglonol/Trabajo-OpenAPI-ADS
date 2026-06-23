namespace PublicidadMarketplaceAnunciantes.Models;

/// <summary>Targeting (uno-a-uno) de una campaña (RF-F4). Listas como CSV.</summary>
public class Targeting
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }

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
