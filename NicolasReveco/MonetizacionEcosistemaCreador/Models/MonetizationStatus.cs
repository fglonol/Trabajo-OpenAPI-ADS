namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Activación/desactivación de monetización a nivel canal o contenido (RF-M1).</summary>
public class MonetizationStatus
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public MonetizationResourceType ResourceType { get; set; }
    public bool Enabled { get; set; } = true;
    public string? Reason { get; set; }
    public DateTime UpdatedAt { get; set; }
}
