using EnviosRapidosGT.Api.Models;

namespace EnviosRapidosGT.Api.Services;

public interface ITarifaService
{
    decimal CalcularTarifaBase(decimal pesoKg);
    decimal AplicarDescuentoNit(decimal tarifaBase, string? nitRemitente, string? nitDestinatario);
    bool AplicaDescuento(string? nitRemitente, string? nitDestinatario);
}

/// <summary>
/// Calcula tarifas según el peso (Regla 1) y aplica el descuento por NIT (Regla 7).
/// </summary>
public class TarifaService : ITarifaService
{
    public const decimal PorcentajeDescuentoNit = 0.05m;

    /// <summary>
    /// Tarifa por peso:
    /// &lt;= 1kg = Q25, 1.01-5kg = Q45, 5.01-10kg = Q75, &gt; 10kg = Q100.
    /// </summary>
    public decimal CalcularTarifaBase(decimal pesoKg)
    {
        if (pesoKg <= 0)
            throw new ReglaNegocioException("El peso del envío debe ser mayor a 0 kg.");

        return pesoKg switch
        {
            <= 1m => 25m,
            <= 5m => 45m,
            <= 10m => 75m,
            _ => 100m
        };
    }

    /// <summary>El descuento aplica si el remitente O el destinatario tiene NIT válido.</summary>
    public bool AplicaDescuento(string? nitRemitente, string? nitDestinatario)
        => NitValidator.EsValido(nitRemitente) || NitValidator.EsValido(nitDestinatario);

    public decimal AplicarDescuentoNit(decimal tarifaBase, string? nitRemitente, string? nitDestinatario)
    {
        if (!AplicaDescuento(nitRemitente, nitDestinatario))
            return tarifaBase;

        var final = tarifaBase * (1 - PorcentajeDescuentoNit);
        return Math.Round(final, 2, MidpointRounding.AwayFromZero);
    }
}
