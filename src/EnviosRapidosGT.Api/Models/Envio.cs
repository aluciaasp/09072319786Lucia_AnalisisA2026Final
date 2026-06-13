using System.ComponentModel.DataAnnotations;

namespace EnviosRapidosGT.Api.Models;

/// <summary>
/// Representa un paquete/envío gestionado por Envíos Rápidos GT.
/// </summary>
public class Envio
{
    public int Id { get; set; }

    /// <summary>Código de rastreo autogenerado con formato ENV-YYYYMMDD-XXXX.</summary>
    [Required]
    [MaxLength(20)]
    public string CodigoRastreo { get; set; } = string.Empty;

    // --- Remitente ---
    public int RemitenteId { get; set; }
    public Cliente Remitente { get; set; } = null!;

    // --- Destinatario ---
    public int DestinatarioId { get; set; }
    public Cliente Destinatario { get; set; } = null!;

    /// <summary>Peso del paquete en kilogramos.</summary>
    public decimal PesoKg { get; set; }

    /// <summary>Departamento/oficina de destino (cobertura en 18 departamentos).</summary>
    [MaxLength(80)]
    public string DepartamentoDestino { get; set; } = string.Empty;

    // --- Tarifa ---
    public decimal TarifaBase { get; set; }
    public bool DescuentoNitAplicado { get; set; }
    public decimal TarifaFinal { get; set; }

    // --- Estado ---
    public EstadoEnvio EstadoActual { get; set; } = EstadoEnvio.Registrado;

    /// <summary>Oficina donde se realizó la última actualización de estado.</summary>
    [MaxLength(80)]
    public string UbicacionActual { get; set; } = string.Empty;

    /// <summary>Intentos de entrega realizados (máximo 3).</summary>
    public int IntentosEntrega { get; set; }

    public DateTime FechaRegistro { get; set; }

    public List<HistorialEstado> Historial { get; set; } = new();
}
