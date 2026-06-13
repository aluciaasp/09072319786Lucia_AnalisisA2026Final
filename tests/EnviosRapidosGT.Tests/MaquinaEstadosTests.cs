using EnviosRapidosGT.Api.Models;
using EnviosRapidosGT.Api.Services;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class MaquinaEstadosTests
{
    // Regla 3: transiciones válidas (avance en una dirección).
    [Theory]
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.EnTransito)]
    [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.EnReparto)]
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Devuelto)]
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.EnDevolucion)]
    [InlineData(EstadoEnvio.EnDevolucion, EstadoEnvio.Devuelto)]
    public void PuedeTransicionar_TransicionesValidas_DevuelveTrue(EstadoEnvio actual, EstadoEnvio destino)
    {
        Assert.True(MaquinaEstados.PuedeTransicionar(actual, destino));
    }

    // Transiciones inválidas: retroceder o saltar etapas.
    [Theory]
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.EnReparto)]   // salto
    [InlineData(EstadoEnvio.Registrado, EstadoEnvio.Entregado)]   // salto
    [InlineData(EstadoEnvio.EnReparto, EstadoEnvio.Registrado)]   // retroceso
    [InlineData(EstadoEnvio.EnTransito, EstadoEnvio.Registrado)]  // retroceso
    [InlineData(EstadoEnvio.Entregado, EstadoEnvio.EnReparto)]    // desde final
    public void PuedeTransicionar_TransicionesInvalidas_DevuelveFalse(EstadoEnvio actual, EstadoEnvio destino)
    {
        Assert.False(MaquinaEstados.PuedeTransicionar(actual, destino));
    }

    [Theory]
    [InlineData(EstadoEnvio.Entregado)]
    [InlineData(EstadoEnvio.Devuelto)]
    public void EsFinal_EstadosFinales_DevuelveTrue(EstadoEnvio estado)
    {
        Assert.True(MaquinaEstados.EsFinal(estado));
    }

    [Fact]
    public void ValidarTransicion_DesdeEstadoFinal_Lanza()
    {
        Assert.Throws<ReglaNegocioException>(
            () => MaquinaEstados.ValidarTransicion(EstadoEnvio.Entregado, EstadoEnvio.EnReparto));
    }

    [Fact]
    public void ValidarTransicion_TransicionInvalida_Lanza()
    {
        Assert.Throws<ReglaNegocioException>(
            () => MaquinaEstados.ValidarTransicion(EstadoEnvio.Registrado, EstadoEnvio.Entregado));
    }
}
