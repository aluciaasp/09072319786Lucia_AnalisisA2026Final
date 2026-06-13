using EnviosRapidosGT.Api.Models;

namespace EnviosRapidosGT.Api.Dtos;

public static class EnvioMapper
{
    public static EnvioDto ToDto(this Envio e) => new()
    {
        Id = e.Id,
        CodigoRastreo = e.CodigoRastreo,
        RemitenteNombre = e.Remitente?.Nombre ?? string.Empty,
        DestinatarioNombre = e.Destinatario?.Nombre ?? string.Empty,
        PesoKg = e.PesoKg,
        DepartamentoDestino = e.DepartamentoDestino,
        TarifaBase = e.TarifaBase,
        DescuentoNitAplicado = e.DescuentoNitAplicado,
        TarifaFinal = e.TarifaFinal,
        EstadoActual = e.EstadoActual.ToString(),
        UbicacionActual = e.UbicacionActual,
        IntentosEntrega = e.IntentosEntrega,
        FechaRegistro = e.FechaRegistro,
        Historial = e.Historial
            .OrderBy(h => h.Timestamp)
            .Select(h => new HistorialDto
            {
                Estado = h.Estado,
                Ubicacion = h.Ubicacion,
                Timestamp = h.Timestamp,
                Notas = h.Notas
            })
            .ToList()
    };
}
