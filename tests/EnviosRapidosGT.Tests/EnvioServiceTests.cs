using EnviosRapidosGT.Api.Dtos;
using EnviosRapidosGT.Api.Models;
using EnviosRapidosGT.Api.Services;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class EnvioServiceTests
{
    private static CrearEnvioDto NuevoEnvioDto(decimal peso = 2m, string? nitRem = null, string? nitDest = null)
        => new()
        {
            Remitente = new ClienteDto { Nombre = "Remitente", Nit = nitRem },
            Destinatario = new ClienteDto { Nombre = "Destinatario", Nit = nitDest },
            PesoKg = peso,
            DepartamentoDestino = "Quetzaltenango"
        };

    private static (EnvioService service, FakeClock clock) Crear(string nombreDb)
    {
        var db = TestDb.NuevoContexto(nombreDb);
        var clock = new FakeClock();
        var service = new EnvioService(db, new TarifaService(), clock);
        return (service, clock);
    }

    [Fact]
    public async Task RegistrarEnvio_GeneraCodigoConFormatoCorrecto()
    {
        var (service, _) = Crear(nameof(RegistrarEnvio_GeneraCodigoConFormatoCorrecto));

        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        Assert.Matches(@"^ENV-\d{8}-\d{4}$", envio.CodigoRastreo);
        Assert.Equal("ENV-20260613-0001", envio.CodigoRastreo);
    }

    [Fact]
    public async Task RegistrarEnvio_CodigosCorrelativosPorDia()
    {
        var (service, _) = Crear(nameof(RegistrarEnvio_CodigosCorrelativosPorDia));

        var e1 = await service.RegistrarEnvioAsync(NuevoEnvioDto());
        var e2 = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        Assert.Equal("ENV-20260613-0001", e1.CodigoRastreo);
        Assert.Equal("ENV-20260613-0002", e2.CodigoRastreo);
    }

    [Fact]
    public async Task RegistrarEnvio_EstadoInicialRegistrado_ConHistorial()
    {
        var (service, _) = Crear(nameof(RegistrarEnvio_EstadoInicialRegistrado_ConHistorial));

        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        Assert.Equal(EstadoEnvio.Registrado, envio.EstadoActual);
        Assert.Single(envio.Historial);
        Assert.Equal(EstadoEnvio.Registrado, envio.Historial[0].Estado);
    }

    [Fact]
    public async Task RegistrarEnvio_ConNitValido_AplicaDescuento()
    {
        var (service, _) = Crear(nameof(RegistrarEnvio_ConNitValido_AplicaDescuento));

        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto(peso: 2m, nitRem: "1234567-9"));

        Assert.True(envio.DescuentoNitAplicado);
        Assert.Equal(45m, envio.TarifaBase);
        Assert.Equal(42.75m, envio.TarifaFinal);
    }

    [Fact]
    public async Task ActualizarEstado_AvanceValido_ActualizaYRegistraHistorial()
    {
        var (service, _) = Crear(nameof(ActualizarEstado_AvanceValido_ActualizaYRegistraHistorial));
        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        var actualizado = await service.ActualizarEstadoAsync(envio.Id, new ActualizarEstadoDto
        {
            NuevoEstado = EstadoEnvio.EnTransito,
            Ubicacion = "Oficina Central"
        });

        Assert.Equal(EstadoEnvio.EnTransito, actualizado.EstadoActual);
        Assert.Equal("Oficina Central", actualizado.UbicacionActual);
        Assert.Equal(2, actualizado.Historial.Count);
    }

    [Fact]
    public async Task ActualizarEstado_TransicionInvalida_Lanza()
    {
        var (service, _) = Crear(nameof(ActualizarEstado_TransicionInvalida_Lanza));
        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            service.ActualizarEstadoAsync(envio.Id, new ActualizarEstadoDto
            {
                NuevoEstado = EstadoEnvio.Entregado, // salto inválido desde Registrado
                Ubicacion = "Oficina Central"
            }));
    }

    [Fact]
    public async Task IntentoFallido_TercerIntento_PasaAutomaticamenteAEnDevolucion()
    {
        var (service, _) = Crear(nameof(IntentoFallido_TercerIntento_PasaAutomaticamenteAEnDevolucion));
        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        // Avanzar hasta EnReparto.
        await service.ActualizarEstadoAsync(envio.Id, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnTransito, Ubicacion = "Central" });
        await service.ActualizarEstadoAsync(envio.Id, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnReparto, Ubicacion = "Xela" });

        await service.RegistrarIntentoFallidoAsync(envio.Id, new IntentoFallidoDto { Ubicacion = "Xela" });
        await service.RegistrarIntentoFallidoAsync(envio.Id, new IntentoFallidoDto { Ubicacion = "Xela" });
        var trasTercero = await service.RegistrarIntentoFallidoAsync(envio.Id, new IntentoFallidoDto { Ubicacion = "Xela" });

        Assert.Equal(3, trasTercero.IntentosEntrega);
        Assert.Equal(EstadoEnvio.EnDevolucion, trasTercero.EstadoActual);
    }

    [Fact]
    public async Task IntentoFallido_FueraDeReparto_Lanza()
    {
        var (service, _) = Crear(nameof(IntentoFallido_FueraDeReparto_Lanza));
        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        await Assert.ThrowsAsync<ReglaNegocioException>(() =>
            service.RegistrarIntentoFallidoAsync(envio.Id, new IntentoFallidoDto { Ubicacion = "Central" }));
    }

    [Fact]
    public async Task ObtenerPorCodigo_DevuelveEnvioCorrecto()
    {
        var (service, _) = Crear(nameof(ObtenerPorCodigo_DevuelveEnvioCorrecto));
        var envio = await service.RegistrarEnvioAsync(NuevoEnvioDto());

        var encontrado = await service.ObtenerPorCodigoAsync(envio.CodigoRastreo);

        Assert.NotNull(encontrado);
        Assert.Equal(envio.Id, encontrado!.Id);
    }

    [Fact]
    public async Task GenerarReporte_CalculaPorcentajes()
    {
        var (service, _) = Crear(nameof(GenerarReporte_CalculaPorcentajes));

        // Envío 1 -> Entregado
        var e1 = await service.RegistrarEnvioAsync(NuevoEnvioDto());
        await service.ActualizarEstadoAsync(e1.Id, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnTransito, Ubicacion = "C" });
        await service.ActualizarEstadoAsync(e1.Id, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.EnReparto, Ubicacion = "C" });
        await service.ActualizarEstadoAsync(e1.Id, new ActualizarEstadoDto { NuevoEstado = EstadoEnvio.Entregado, Ubicacion = "C" });

        // Envío 2 -> En proceso (Registrado)
        await service.RegistrarEnvioAsync(NuevoEnvioDto());

        var reporte = await service.GenerarReporteAsync();

        Assert.Equal(2, reporte.TotalEnvios);
        Assert.Equal(1, reporte.Entregados);
        Assert.Equal(1, reporte.EnProceso);
        Assert.Equal(50m, reporte.PorcentajeEntrega);
    }
}
