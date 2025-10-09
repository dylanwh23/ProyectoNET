using MassTransit;
using ProyectoNET.Shared;

namespace ProyectoNET.Carreras.API.Consumers;

public class TiempoRegistradoConsumer(ILogger<TiempoRegistradoConsumer> logger)
    : IConsumer<ProgresoCorredorActualizado>
{
    public Task Consume(ConsumeContext<ProgresoCorredorActualizado> context)
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

        // Log detallado de los tiempos por tramo (opcional, útil para debugging)
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

            // Calcular tiempo total acumulado
            var tiempoTotal = TimeSpan.FromTicks(mensaje.TiemposPorTramo.Sum(t => t.Tiempo.Ticks));
            logger.LogDebug(
                "  🏃 Tiempo total acumulado: {TiempoTotal}",
                tiempoTotal.ToString(@"hh\:mm\:ss")
            );
        }

        // AQUÍ iría tu lógica de negocio: guardar en la base de datos, etc.

        return Task.CompletedTask;
    }
}