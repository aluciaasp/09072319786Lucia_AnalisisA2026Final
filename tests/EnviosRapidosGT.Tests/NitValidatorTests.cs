using EnviosRapidosGT.Api.Services;
using Xunit;

namespace EnviosRapidosGT.Tests;

public class NitValidatorTests
{
    [Theory]
    [InlineData("1234567-9")]   // válido (módulo 11)
    [InlineData("12345679")]    // mismo NIT sin guion
    public void EsValido_NitCorrecto_DevuelveTrue(string nit)
    {
        Assert.True(NitValidator.EsValido(nit));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("CF")]          // consumidor final
    [InlineData("cf")]
    [InlineData("1234567-8")]   // dígito verificador incorrecto
    [InlineData("ABC123")]      // formato inválido
    public void EsValido_NitInvalido_DevuelveFalse(string? nit)
    {
        Assert.False(NitValidator.EsValido(nit));
    }
}
