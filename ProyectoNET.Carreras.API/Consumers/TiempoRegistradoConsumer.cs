using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared;

namespace ProyectoNET.Carreras.API.Consumers;

public class TiempoRegistradoConsumer(
    ILogger<TiempoRegistradoConsumer> logger,
    IHubContext<CarreraHub> hubContext
) : IConsumer<ProgresoCorredorActualizado>
{
    // DTO compatible con la clase CarreraData definida en el cliente Blazor
    // Se recomienda definir este DTO en un Shared/DTOs si se usa en varios lugares
    private class CarreraData
    {
        public int CarreraId { get; set; }
        public int CorredorId { get; set; }
        public string Checkpoint { get; set; } = string.Empty;
        public double Velocidad { get; set; }
        public int TramosCompletados { get; set; }
    }

    public async Task Consume(ConsumeContext<ProgresoCorredorActualizado> context)
    {
        var mensaje = context.Message;

        logger.LogInformation(
            "📥 Progreso recibido - Carrera {IdCarrera} | Corredor {IdCorredor} | Checkpoint {Km}km (ID: {IdCheckpoint}) | Velocidad {Velocidad:F2} km/h | Tramos completados: {CantidadTramos}",
            mensaje.IdCarrera,
            mensaje.IdCorredor,
            mensaje.UltimoCheckpointPasado.Km,
            mensaje.UltimoCheckpointPasado.IdPuntoDeControl,
            mensaje.VelocidadKmh,
            mensaje.TiemposPorTramo.Count
        );

        // 1. Mapear el DTO complejo de MassTransit al DTO simple esperado por el cliente Blazor.
        var dataParaCliente = new CarreraData
        {
            CarreraId = mensaje.IdCarrera,
            CorredorId = mensaje.IdCorredor,
            // Concatenar los datos del checkpoint a un string, como espera el cliente
            Checkpoint = $"{mensaje.UltimoCheckpointPasado.Km}km ({mensaje.UltimoCheckpointPasado.IdPuntoDeControl})",
            Velocidad = mensaje.VelocidadKmh,
            TramosCompletados = mensaje.TiemposPorTramo.Count
        };

        // 2. 🔄 Enviar a los clientes conectados por SignalR.
        // Se corrige el nombre del método a "RecibirProgreso" (el que espera el cliente).
        await hubContext.Clients.Group($"carrera-{mensaje.IdCarrera}")
                 .SendAsync("RecibirProgreso", dataParaCliente);
        // Log detallado (debug)
        if (logger.IsEnabled(LogLevel.Debug))
        {
            // ... (el resto de tu lógica de log sigue igual)
            foreach (var tramo in mensaje.TiemposPorTramo)
            {
                logger.LogDebug(
                    "  ⏱️ Tramo {Desde} → {Hasta}: {Tiempo}",
                    tramo.DesdePuntoDeControlId,
                    tramo.HastaPuntoDeControlId,
                    tramo.Tiempo.ToString(@"hh\:mm\:ss")
                );
            }

            var tiempoTotal = TimeSpan.FromTicks(mensaje.TiemposPorTramo.Sum(t => t.Tiempo.Ticks));
            logger.LogDebug("  🏃 Tiempo total acumulado: {TiempoTotal}", tiempoTotal.ToString(@"hh\:mm\:ss"));
        }
    }
}