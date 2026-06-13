using EnviosRapidosGT.Api.Dtos;
using EnviosRapidosGT.Api.Models;
using EnviosRapidosGT.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnviosRapidosGT.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnviosController : ControllerBase
{
    private readonly IEnvioService _service;

    public EnviosController(IEnvioService service) => _service = service;

    /// <summary>Endpoint 1: Registra un nuevo envío (calcula tarifa y genera código de rastreo).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnvioDto>> Crear([FromBody] CrearEnvioDto dto)
    {
        var envio = await _service.RegistrarEnvioAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = envio.Id }, envio.ToDto());
    }

    /// <summary>Endpoint 2: Lista todos los envíos, con filtro opcional por estado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnvioDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnvioDto>>> Listar([FromQuery] EstadoEnvio? estado)
    {
        var envios = await _service.ListarAsync(estado);
        return Ok(envios.Select(e => e.ToDto()));
    }

    /// <summary>Endpoint 3: Obtiene un envío por su Id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDto>> ObtenerPorId(int id)
    {
        var envio = await _service.ObtenerPorIdAsync(id);
        return envio is null ? NotFound() : Ok(envio.ToDto());
    }

    /// <summary>Endpoint 4: Rastrea un envío por su código (ENV-YYYYMMDD-XXXX).</summary>
    [HttpGet("rastreo/{codigo}")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDto>> RastrearPorCodigo(string codigo)
    {
        var envio = await _service.ObtenerPorCodigoAsync(codigo);
        return envio is null ? NotFound() : Ok(envio.ToDto());
    }

    /// <summary>Endpoint 5: Consulta el historial completo de estados de un envío.</summary>
    [HttpGet("{id:int}/historial")]
    [ProducesResponseType(typeof(IEnumerable<HistorialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<HistorialDto>>> Historial(int id)
    {
        var envio = await _service.ObtenerPorIdAsync(id);
        return envio is null ? NotFound() : Ok(envio.ToDto().Historial);
    }

    /// <summary>Endpoint 6: Actualiza el estado de un envío (respeta la máquina de estados).</summary>
    [HttpPut("{id:int}/estado")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDto>> ActualizarEstado(int id, [FromBody] ActualizarEstadoDto dto)
    {
        var envio = await _service.ActualizarEstadoAsync(id, dto);
        return Ok(envio.ToDto());
    }

    /// <summary>Endpoint 7: Registra un intento de entrega fallido (al 3.º pasa a EnDevolucion).</summary>
    [HttpPost("{id:int}/intento-fallido")]
    [ProducesResponseType(typeof(EnvioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnvioDto>> IntentoFallido(int id, [FromBody] IntentoFallidoDto dto)
    {
        var envio = await _service.RegistrarIntentoFallidoAsync(id, dto);
        return Ok(envio.ToDto());
    }

    /// <summary>Endpoint 8: Reporte de eficiencia de entrega (entregados vs devueltos).</summary>
    [HttpGet("/api/reportes/eficiencia")]
    [ProducesResponseType(typeof(ReporteEficienciaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReporteEficienciaDto>> ReporteEficiencia()
        => Ok(await _service.GenerarReporteAsync());
}
