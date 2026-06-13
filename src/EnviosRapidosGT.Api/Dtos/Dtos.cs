using System.ComponentModel.DataAnnotations;
using EnviosRapidosGT.Api.Models;

namespace EnviosRapidosGT.Api.Dtos;

/// <summary>Datos de un cliente al registrar un envío.</summary>
public class ClienteDto
{
    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Nit { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }
}

/// <summary>Petición para registrar un nuevo envío.</summary>
public class CrearEnvioDto
{
    [Required]
    public ClienteDto Remitente { get; set; } = new();

    [Required]
    public ClienteDto Destinatario { get; set; } = new();

    [Range(0.01, 1000, ErrorMessage = "El peso debe ser mayor a 0 kg.")]
    public decimal PesoKg { get; set; }

    [Required]
    [MaxLength(80)]
    public string DepartamentoDestino { get; set; } = string.Empty;
}

/// <summary>Petición para actualizar el estado de un envío.</summary>
public class ActualizarEstadoDto
{
    [Required]
    public EstadoEnvio NuevoEstado { get; set; }

    [Required(ErrorMessage = "La ubicación (oficina) es obligatoria.")]
    [MaxLength(80)]
    public string Ubicacion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Notas { get; set; }
}

/// <summary>Petición para registrar un intento de entrega fallido.</summary>
public class IntentoFallidoDto
{
    [Required(ErrorMessage = "La ubicación (oficina) es obligatoria.")]
    [MaxLength(80)]
    public string Ubicacion { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Notas { get; set; }
}

// ---------- Respuestas ----------

public class HistorialDto
{
    public EstadoEnvio Estado { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Notas { get; set; }
}

public class EnvioDto
{
    public int Id { get; set; }
    public string CodigoRastreo { get; set; } = string.Empty;
    public string RemitenteNombre { get; set; } = string.Empty;
    public string DestinatarioNombre { get; set; } = string.Empty;
    public decimal PesoKg { get; set; }
    public string DepartamentoDestino { get; set; } = string.Empty;
    public decimal TarifaBase { get; set; }
    public bool DescuentoNitAplicado { get; set; }
    public decimal TarifaFinal { get; set; }
    public string EstadoActual { get; set; } = string.Empty;
    public string UbicacionActual { get; set; } = string.Empty;
    public int IntentosEntrega { get; set; }
    public DateTime FechaRegistro { get; set; }
    public List<HistorialDto> Historial { get; set; } = new();
}

public class ReporteEficienciaDto
{
    public int TotalEnvios { get; set; }
    public int Entregados { get; set; }
    public int Devueltos { get; set; }
    public int EnProceso { get; set; }
    public decimal PorcentajeEntrega { get; set; }
    public decimal PorcentajeDevolucion { get; set; }
    public int EnviosConIntentosFallidos { get; set; }
}
