namespace DescubrimientoPersonalizacion.Models;

/// <summary>
/// Interés inferido de un usuario, derivado de las etiquetas del contenido con el que interactúa.
/// Se usa para personalizar el feed (RF-D2).
/// </summary>
public class UserInterest
{
    public Guid Id { get; set; }

    /// <summary>Usuario dueño del interés.</summary>
    public Guid UserId { get; set; }

    /// <summary>Interés (normalmente el valor de una etiqueta de contenido).</summary>
    public string Interest { get; set; } = string.Empty;

    /// <summary>Puntaje acumulado del interés (mayor = más relevante para el usuario).</summary>
    public double Score { get; set; }

    public DateTime UpdatedAt { get; set; }
}
