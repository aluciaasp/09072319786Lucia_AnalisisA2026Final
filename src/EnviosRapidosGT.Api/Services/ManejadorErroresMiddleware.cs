using System.Text.Json;

namespace EnviosRapidosGT.Api.Services;

/// <summary>
/// Traduce las excepciones de regla de negocio a respuestas HTTP 400 con un
/// cuerpo JSON uniforme, evitando filtrar detalles internos.
/// </summary>
public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;

    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ReglaNegocioException ex)
        {
            _logger.LogWarning(ex, "Regla de negocio violada");
            await EscribirError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await EscribirError(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error inesperado.");
        }
    }

    private static async Task EscribirError(HttpContext context, int status, string mensaje)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { error = mensaje, status });
        await context.Response.WriteAsync(payload);
    }
}
