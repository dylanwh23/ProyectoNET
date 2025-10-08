using MassTransit;
using ProyectoNET.Shared;

namespace ProyectoNET.Carreras.API.Consumers;

public class TiempoRegistradoConsumer(ILogger<TiempoRegistradoConsumer> logger)
    : IConsumer<TiempoRegistrado>
{
    public Task Consume(ConsumeContext<TiempoRegistrado> context)
    {
        var mensaje = context.Message;

        logger.LogInformation(
            "📥 Tiempo recibido para la carrera {IdCarrera} y corredor {IdCorredor} en el punto {PuntoDeControl}",
            mensaje.IdCarrera,
            mensaje.IdCorredor,
            mensaje.PuntoDeControl
        );

        // AQUÍ iría tu lógica de negocio: guardar en la base de datos, etc.

        return Task.CompletedTask;
    }
}