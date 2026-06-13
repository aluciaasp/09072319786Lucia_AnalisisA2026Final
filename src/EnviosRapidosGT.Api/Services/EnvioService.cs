using EnviosRapidosGT.Api.Data;
using EnviosRapidosGT.Api.Dtos;
using EnviosRapidosGT.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EnviosRapidosGT.Api.Services;

public interface IEnvioService
{
    Task<Envio> RegistrarEnvioAsync(CrearEnvioDto dto);
    Task<Envio> ActualizarEstadoAsync(int envioId, ActualizarEstadoDto dto);
    Task<Envio> RegistrarIntentoFallidoAsync(int envioId, IntentoFallidoDto dto);
    Task<Envio?> ObtenerPorIdAsync(int id);
    Task<Envio?> ObtenerPorCodigoAsync(string codigo);
    Task<List<Envio>> ListarAsync(EstadoEnvio? estado = null);
    Task<ReporteEficienciaDto> GenerarReporteAsync();
}

/// <summary>
/// Orquesta las reglas de negocio sobre envíos: cálculo de tarifa, generación
/// del código de rastreo, máquina de estados, intentos de entrega e historial.
/// </summary>
public class EnvioService : IEnvioService
{
    public const int MaxIntentosEntrega = 3;

    private readonly AppDbContext _db;
    private readonly ITarifaService _tarifa;
    private readonly IClock _clock;

    public EnvioService(AppDbContext db, ITarifaService tarifa, IClock clock)
    {
        _db = db;
        _tarifa = tarifa;
        _clock = clock;
    }

    public async Task<Envio> RegistrarEnvioAsync(CrearEnvioDto dto)
    {
        var ahora = _clock.Now;

        var tarifaBase = _tarifa.CalcularTarifaBase(dto.PesoKg);
        var aplicaDescuento = _tarifa.AplicaDescuento(dto.Remitente.Nit, dto.Destinatario.Nit);
        var tarifaFinal = _tarifa.AplicarDescuentoNit(tarifaBase, dto.Remitente.Nit, dto.Destinatario.Nit);

        var ubicacionInicial = string.IsNullOrWhiteSpace(dto.DepartamentoDestino)
            ? "Oficina Central"
            : $"Oficina {dto.DepartamentoDestino}";

        var envio = new Envio
        {
            CodigoRastreo = await GenerarCodigoRastreoAsync(ahora),
            Remitente = MapCliente(dto.Remitente),
            Destinatario = MapCliente(dto.Destinatario),
            PesoKg = dto.PesoKg,
            DepartamentoDestino = dto.DepartamentoDestino,
            TarifaBase = tarifaBase,
            DescuentoNitAplicado = aplicaDescuento,
            TarifaFinal = tarifaFinal,
            EstadoActual = EstadoEnvio.Registrado,
            UbicacionActual = ubicacionInicial,
            IntentosEntrega = 0,
            FechaRegistro = ahora
        };

        envio.Historial.Add(new HistorialEstado
        {
            Estado = EstadoEnvio.Registrado,
            Ubicacion = ubicacionInicial,
            Timestamp = ahora,
            Notas = "Envío registrado."
        });

        _db.Envios.Add(envio);
        await _db.SaveChangesAsync();
        return envio;
    }

    public async Task<Envio> ActualizarEstadoAsync(int envioId, ActualizarEstadoDto dto)
    {
        var envio = await CargarConHistorialAsync(envioId)
            ?? throw new ReglaNegocioException($"No existe un envío con Id {envioId}.");

        // Regla 3: validar transición permitida.
        MaquinaEstados.ValidarTransicion(envio.EstadoActual, dto.NuevoEstado);

        AplicarCambioEstado(envio, dto.NuevoEstado, dto.Ubicacion, dto.Notas);

        await _db.SaveChangesAsync();
        return envio;
    }

    public async Task<Envio> RegistrarIntentoFallidoAsync(int envioId, IntentoFallidoDto dto)
    {
        var envio = await CargarConHistorialAsync(envioId)
            ?? throw new ReglaNegocioException($"No existe un envío con Id {envioId}.");

        // Los intentos de entrega solo ocurren cuando el envío está en reparto.
        if (envio.EstadoActual != EstadoEnvio.EnReparto)
            throw new ReglaNegocioException(
                $"Solo se pueden registrar intentos de entrega cuando el envío está 'EnReparto' (estado actual: '{envio.EstadoActual}').");

        if (envio.IntentosEntrega >= MaxIntentosEntrega)
            throw new ReglaNegocioException("El envío ya alcanzó el máximo de 3 intentos de entrega.");

        envio.IntentosEntrega++;
        var ahora = _clock.Now;

        envio.Historial.Add(new HistorialEstado
        {
            Estado = envio.EstadoActual,
            Ubicacion = dto.Ubicacion,
            Timestamp = ahora,
            Notas = $"Intento de entrega #{envio.IntentosEntrega} fallido." +
                    (string.IsNullOrWhiteSpace(dto.Notas) ? "" : $" {dto.Notas}")
        });

        // Regla 2: al fallar el tercer intento, pasa automáticamente a EnDevolucion.
        if (envio.IntentosEntrega >= MaxIntentosEntrega)
        {
            AplicarCambioEstado(envio, EstadoEnvio.EnDevolucion, dto.Ubicacion,
                "Tercer intento fallido: el envío pasa automáticamente a devolución.");
        }

        await _db.SaveChangesAsync();
        return envio;
    }

    public Task<Envio?> ObtenerPorIdAsync(int id) => CargarConHistorialAsync(id);

    public Task<Envio?> ObtenerPorCodigoAsync(string codigo) =>
        _db.Envios
            .Include(e => e.Remitente)
            .Include(e => e.Destinatario)
            .Include(e => e.Historial.OrderBy(h => h.Timestamp))
            .FirstOrDefaultAsync(e => e.CodigoRastreo == codigo);

    public async Task<List<Envio>> ListarAsync(EstadoEnvio? estado = null)
    {
        var query = _db.Envios
            .Include(e => e.Remitente)
            .Include(e => e.Destinatario)
            .Include(e => e.Historial)
            .AsQueryable();

        if (estado.HasValue)
            query = query.Where(e => e.EstadoActual == estado.Value);

        return await query.OrderByDescending(e => e.FechaRegistro).ToListAsync();
    }

    public async Task<ReporteEficienciaDto> GenerarReporteAsync()
    {
        var total = await _db.Envios.CountAsync();
        var entregados = await _db.Envios.CountAsync(e => e.EstadoActual == EstadoEnvio.Entregado);
        var devueltos = await _db.Envios.CountAsync(e => e.EstadoActual == EstadoEnvio.Devuelto);
        var conFallidos = await _db.Envios.CountAsync(e => e.IntentosEntrega > 0);

        decimal Pct(int parte) => total == 0 ? 0 : Math.Round(parte * 100m / total, 2);

        return new ReporteEficienciaDto
        {
            TotalEnvios = total,
            Entregados = entregados,
            Devueltos = devueltos,
            EnProceso = total - entregados - devueltos,
            PorcentajeEntrega = Pct(entregados),
            PorcentajeDevolucion = Pct(devueltos),
            EnviosConIntentosFallidos = conFallidos
        };
    }

    // ---------- Helpers privados ----------

    private void AplicarCambioEstado(Envio envio, EstadoEnvio nuevo, string ubicacion, string? notas)
    {
        var ahora = _clock.Now;
        envio.EstadoActual = nuevo;
        envio.UbicacionActual = ubicacion;

        // Regla 6: cada cambio se registra en el historial.
        envio.Historial.Add(new HistorialEstado
        {
            Estado = nuevo,
            Ubicacion = ubicacion,
            Timestamp = ahora,
            Notas = notas
        });
    }

    private Task<Envio?> CargarConHistorialAsync(int id) =>
        _db.Envios
            .Include(e => e.Remitente)
            .Include(e => e.Destinatario)
            .Include(e => e.Historial)
            .FirstOrDefaultAsync(e => e.Id == id);

    private static Cliente MapCliente(ClienteDto dto) => new()
    {
        Nombre = dto.Nombre,
        Nit = dto.Nit,
        Telefono = dto.Telefono,
        Direccion = dto.Direccion
    };

    /// <summary>
    /// Genera el código ENV-YYYYMMDD-XXXX donde XXXX es un correlativo
    /// diario de 4 dígitos.
    /// </summary>
    private async Task<string> GenerarCodigoRastreoAsync(DateTime fecha)
    {
        var prefijo = $"ENV-{fecha:yyyyMMdd}-";
        var delDia = await _db.Envios.CountAsync(e => e.CodigoRastreo.StartsWith(prefijo));
        var correlativo = (delDia + 1).ToString("D4");
        return prefijo + correlativo;
    }
}
