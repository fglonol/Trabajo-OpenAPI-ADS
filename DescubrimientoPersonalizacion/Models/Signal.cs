namespace DescubrimientoPersonalizacion.Models;

/// <summary>Señal de comportamiento capturada de un usuario sobre un contenido (RF-D5).</summary>
public class Signal
{
    public Guid Id { get; set; }

    /// <summary>Usuario que generó la señal.</summary>
    public Guid UserId { get; set; }

    /// <summary>Contenido objetivo de la señal (referencia a IndexedContent.Id / catalogItemId).</summary>
    public Guid ContentId { get; set; }

    /// <summary>Tipo de señal (View, Click, Like, Search, WatchTime). Se persiste como string.</summary>
    public SignalType Type { get; set; }

    /// <summary>Peso de la señal, sumado a los puntajes de ranking/tendencia del contenido.</summary>
    public double Weight { get; set; }

    public DateTime CreatedAt { get; set; }
}
