using MassTransit;
using ProyectoNET.Shared;
namespace ProyectoNET.SimulatorWorker;

public class Worker(ILogger<Worker> logger, IBus bus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();

        while (!stoppingToken.IsCancellationRequested)
        {
            // Creamos un evento de tiempo simulado
            var eventoTiempo = new TiempoRegistrado(
                IdCarrera: 1,
                IdCorredor: random.Next(100, 200),
                Tiempo: DateTime.UtcNow,
                PuntoDeControl: 1
            );

            // Publicamos el evento al bus de mensajería
            await bus.Publish(eventoTiempo, stoppingToken);

            logger.LogInformation("✅ Evento de tiempo publicado para el corredor {IdCorredor}", eventoTiempo.IdCorredor);

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}