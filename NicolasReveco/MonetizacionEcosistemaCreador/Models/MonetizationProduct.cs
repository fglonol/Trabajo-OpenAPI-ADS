namespace MonetizacionEcosistemaCreador.Models;

/// <summary>Producto de monetización configurado por un canal: membresía, super thanks o anuncios (RF-M2).</summary>
public class MonetizationProduct
{
    public Guid Id { get; set; }
    public Guid ChannelId { get; set; }
    public ProductType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
