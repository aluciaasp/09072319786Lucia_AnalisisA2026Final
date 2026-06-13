using System.ComponentModel.DataAnnotations;

namespace EnviosRapidosGT.Api.Models;

/// <summary>
/// Registro inmutable de cada cambio de estado de un envío (Regla 6).
/// Incluye nuevo estado, ubicación, timestamp automático y notas opcionales.
/// </summary>
public class HistorialEstado
{
    public int Id { get; set; }

    public int EnvioId { get; set; }
    public Envio? Envio { get; set; }

    public EstadoEnvio Estado { get; set; }

    /// <summary>Oficina donde se actualizó el estado (Regla 4).</summary>
    [Required]
    [MaxLength(80)]
    public string Ubicacion { get; set; } = string.Empty;

    /// <summary>Timestamp automático del cambio.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>Notas opcionales (p. ej. motivo de un intento fallido).</summary>
    [MaxLength(300)]
    public string? Notas { get; set; }
}
