namespace DescubrimientoPersonalizacion.Models.Dtos;

/// <summary>Cuerpo para registrar una señal de comportamiento (RF-D5).</summary>
public class SignalRequest
{
    public Guid UserId { get; set; }

    /// <summary>Contenido objetivo (catalogItemId / IndexedContent.Id).</summary>
    public Guid ContentId { get; set; }

    /// <summary>Tipo de señal: View, Click, Like, Search, WatchTime.</summary>
    public SignalType Type { get; set; }

    /// <summary>Peso de la señal. Si es 0 o negativo se usa un valor por defecto de 1.</summary>
    public double Weight { get; set; }
}
