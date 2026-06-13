using System.Text.RegularExpressions;

namespace EnviosRapidosGT.Api.Services;

/// <summary>
/// Validador de NIT guatemalteco usando el algoritmo oficial de dígito
/// verificador módulo 11. Un NIT válido habilita el descuento del 5% (Regla 7).
/// </summary>
public static class NitValidator
{
    private static readonly Regex Formato = new(@"^\d+[0-9K]$", RegexOptions.Compiled);

    /// <summary>
    /// Indica si el NIT es válido. Valores nulos, vacíos o "CF"
    /// (consumidor final) se consideran inválidos (no aplican descuento).
    /// </summary>
    public static bool EsValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit))
            return false;

        // Normaliza: quita guiones/espacios y pasa a mayúsculas.
        var limpio = nit.Replace("-", "").Replace(" ", "").ToUpperInvariant();

        if (limpio == "CF")
            return false;

        if (!Formato.IsMatch(limpio))
            return false;

        var cuerpo = limpio[..^1];
        var verificador = limpio[^1];

        var suma = 0;
        // Cada dígito se multiplica por un peso decreciente (longitud+1 ... 2).
        for (var i = 0; i < cuerpo.Length; i++)
        {
            var digito = cuerpo[i] - '0';
            var peso = cuerpo.Length + 1 - i;
            suma += digito * peso;
        }

        var modulo = suma % 11;
        var calculado = (11 - modulo) % 11;
        var esperado = calculado == 10 ? 'K' : (char)('0' + calculado);

        return esperado == verificador;
    }
}
