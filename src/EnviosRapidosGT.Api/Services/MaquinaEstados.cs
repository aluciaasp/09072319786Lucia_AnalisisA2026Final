using EnviosRapidosGT.Api.Models;

namespace EnviosRapidosGT.Api.Services;

/// <summary>
/// Máquina de estados del envío (Regla 3). Los estados solo avanzan en una
/// dirección; no se permite retroceder ni saltar etapas.
///
///   Registrado -> EnTransito -> EnReparto -> Entregado
///                                         -> Devuelto
///                                         -> EnDevolucion -> Devuelto
/// </summary>
public static class MaquinaEstados
{
    private static readonly Dictionary<EstadoEnvio, EstadoEnvio[]> Transiciones = new()
    {
        [EstadoEnvio.Registrado] = new[] { EstadoEnvio.EnTransito },
        [EstadoEnvio.EnTransito] = new[] { EstadoEnvio.EnReparto },
        [EstadoEnvio.EnReparto] = new[] { EstadoEnvio.Entregado, EstadoEnvio.EnDevolucion, EstadoEnvio.Devuelto },
        [EstadoEnvio.EnDevolucion] = new[] { EstadoEnvio.Devuelto },
        [EstadoEnvio.Entregado] = Array.Empty<EstadoEnvio>(),
        [EstadoEnvio.Devuelto] = Array.Empty<EstadoEnvio>()
    };

    /// <summary>Estados finales que ya no permiten más cambios.</summary>
    public static bool EsFinal(EstadoEnvio estado)
        => estado is EstadoEnvio.Entregado or EstadoEnvio.Devuelto;

    /// <summary>Indica si se puede pasar de <paramref name="actual"/> a <paramref name="destino"/>.</summary>
    public static bool PuedeTransicionar(EstadoEnvio actual, EstadoEnvio destino)
        => Transiciones.TryGetValue(actual, out var permitidos) && permitidos.Contains(destino);

    /// <summary>Lista de estados a los que se puede avanzar desde el estado actual.</summary>
    public static IReadOnlyList<EstadoEnvio> SiguientesPermitidos(EstadoEnvio actual)
        => Transiciones.TryGetValue(actual, out var permitidos) ? permitidos : Array.Empty<EstadoEnvio>();

    /// <summary>Valida la transición y lanza <see cref="ReglaNegocioException"/> si es inválida.</summary>
    public static void ValidarTransicion(EstadoEnvio actual, EstadoEnvio destino)
    {
        if (EsFinal(actual))
            throw new ReglaNegocioException(
                $"El envío está en estado final '{actual}' y no admite más cambios.");

        if (!PuedeTransicionar(actual, destino))
            throw new ReglaNegocioException(
                $"Transición inválida: no se puede pasar de '{actual}' a '{destino}'.");
    }
}
