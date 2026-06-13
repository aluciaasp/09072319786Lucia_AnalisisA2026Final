namespace EnviosRapidosGT.Api.Services;

/// <summary>
/// Se lanza cuando se viola una regla de negocio (transición inválida,
/// intentos agotados, envío en estado final, etc.). Los controllers la
/// traducen a un HTTP 400/409 con mensaje claro.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
