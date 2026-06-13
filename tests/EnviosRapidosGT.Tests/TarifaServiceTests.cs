using EnviosRapidosGT.Api.Services;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class TarifaServiceTests
{
    private readonly TarifaService _tarifa = new();

    // Regla 1: tarifa por peso. Se prueban los límites de cada rango.
    [Theory]
    [InlineData(0.5, 25)]
    [InlineData(1.0, 25)]      // límite superior del primer rango
    [InlineData(1.01, 45)]     // inicio del segundo rango
    [InlineData(3.0, 45)]
    [InlineData(5.0, 45)]      // límite superior del segundo rango
    [InlineData(5.01, 75)]     // inicio del tercer rango
    [InlineData(10.0, 75)]     // límite superior del tercer rango
    [InlineData(10.01, 100)]   // inicio del cuarto rango
    [InlineData(25.0, 100)]
    public void CalcularTarifaBase_DevuelveTarifaSegunPeso(decimal peso, decimal esperado)
    {
        Assert.Equal(esperado, _tarifa.CalcularTarifaBase(peso));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void CalcularTarifaBase_PesoInvalido_Lanza(decimal peso)
    {
        Assert.Throws<ReglaNegocioException>(() => _tarifa.CalcularTarifaBase(peso));
    }

    [Fact]
    public void AplicarDescuentoNit_ConNitValidoEnRemitente_Aplica5Porciento()
    {
        // NIT 1234567-9 es válido según el algoritmo módulo 11.
        var final = _tarifa.AplicarDescuentoNit(100m, "1234567-9", null);
        Assert.Equal(95m, final);
    }

    [Fact]
    public void AplicarDescuentoNit_ConNitValidoEnDestinatario_Aplica5Porciento()
    {
        var final = _tarifa.AplicarDescuentoNit(45m, null, "1234567-9");
        Assert.Equal(42.75m, final);
    }

    [Fact]
    public void AplicarDescuentoNit_SinNitValido_NoAplica()
    {
        var final = _tarifa.AplicarDescuentoNit(75m, "CF", null);
        Assert.Equal(75m, final);
    }

    [Fact]
    public void AplicaDescuento_AmbosSinNit_EsFalso()
    {
        Assert.False(_tarifa.AplicaDescuento(null, "CF"));
    }
}
