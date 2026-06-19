namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>Cuerpo del upsert de indexación (RF-D6). El <see cref="Id"/> es el catalogItemId de Catálogo.</summary>
public class IndexContentRequest
{
    /// <summary>Identificador del contenido. IGUAL al catalogItemId de Catálogo.</summary>
    public Guid Id { get; set; }

    /// <summary>Canal propietario del contenido.</summary>
    public Guid ChannelId { get; set; }

    /// <summary>Copia ligera del título para búsqueda.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Etiquetas asociadas al contenido.</summary>
    public List<string> Tags { get; set; } = new();
}
