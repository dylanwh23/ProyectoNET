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

        // 🔄 Enviar a los clientes conectados por SignalR
        await hubContext.Clients.All.SendAsync("ProgresoActualizado", mensaje);

        // Log detallado (debug)
        if (logger.IsEnabled(LogLevel.Debug))
        {
            foreach (var tramo in mensaje.TiemposPorTramo)
            {
                logger.LogDebug(
                    "  ⏱️ Tramo {Desde} → {Hasta}: {Tiempo}",
                    tramo.DesdePuntoDeControlId,
                    tramo.HastaPuntoDeControlId,
                    tramo.Tiempo.ToString(@"hh\:mm\:ss")
                );
            }

            var tiempoTotal = TimeSpan.FromTicks(mensaje.TiemposPorTramo.Sum(t => t.Tiempo.Ticks));
            logger.LogDebug("  🏃 Tiempo total acumulado: {TiempoTotal}", tiempoTotal.ToString(@"hh\:mm\:ss"));
        }
    }
}
