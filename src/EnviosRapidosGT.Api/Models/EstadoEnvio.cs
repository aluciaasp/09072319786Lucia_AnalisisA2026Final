namespace EnviosRapidosGT.Api.Models;

/// <summary>
/// Estados posibles de un envío. El orden refleja el avance del flujo.
/// Las transiciones permitidas se definen en <see cref="Services.MaquinaEstados"/>.
/// </summary>
public enum EstadoEnvio
{
    Registrado = 0,
    EnTransito = 1,
    EnReparto = 2,
    Entregado = 3,
    EnDevolucion = 4,
    Devuelto = 5
}
