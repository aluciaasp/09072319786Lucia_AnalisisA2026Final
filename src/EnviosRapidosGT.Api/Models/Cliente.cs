using System.ComponentModel.DataAnnotations;

namespace EnviosRapidosGT.Api.Models;

/// <summary>
/// Cliente que participa en un envío (remitente o destinatario).
/// El NIT es opcional; si es válido se aplica el descuento del 5%.
/// </summary>
public class Cliente
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>NIT del cliente. Puede ser null o "CF" (consumidor final).</summary>
    [MaxLength(20)]
    public string? Nit { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [MaxLength(200)]
    public string? Direccion { get; set; }
}
