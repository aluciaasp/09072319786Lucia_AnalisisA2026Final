namespace EnviosRapidosGT.Api.Services;

/// <summary>
/// Abstracción del reloj para permitir timestamps deterministas en pruebas.
/// </summary>
public interface IClock
{
    DateTime Now { get; }
}

/// <summary>Reloj de sistema (hora de Guatemala, UTC-6).</summary>
public class SystemClock : IClock
{
    private static readonly TimeSpan GuatemalaOffset = TimeSpan.FromHours(-6);
    public DateTime Now => DateTime.UtcNow + GuatemalaOffset;
}
