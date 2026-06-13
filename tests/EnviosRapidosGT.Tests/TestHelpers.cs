using EnviosRapidosGT.Api.Data;
using EnviosRapidosGT.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EnviosRapidosGT.Tests;

/// <summary>Reloj fijo para timestamps deterministas en pruebas.</summary>
public class FakeClock : IClock
{
    public DateTime Now { get; set; } = new DateTime(2026, 6, 13, 8, 0, 0);
}

public static class TestDb
{
    /// <summary>Crea un AppDbContext con base de datos InMemory aislada por nombre.</summary>
    public static AppDbContext NuevoContexto(string nombre)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nombre)
            .EnableSensitiveDataLogging()
            .Options;
        return new AppDbContext(options);
    }
}
