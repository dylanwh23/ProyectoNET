using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Shared;

namespace ProyectoNET.Carreras.API.Consumers;

public class CarreraFinalizadaConsumer : IConsumer<ProyectoNET.Shared.CarreraFinalizadaEvent>
{
    private readonly ICarreraRepository _carreraRepository;
    private readonly ILogger<CarreraFinalizadaConsumer> _logger;
    private readonly IHubContext<CarreraHub> _hubContext;

    public CarreraFinalizadaConsumer(
        ICarreraRepository carreraRepository,
        ILogger<CarreraFinalizadaConsumer> logger,
        IHubContext<CarreraHub> hubContext)
    {
        _carreraRepository = carreraRepository;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<ProyectoNET.Shared.CarreraFinalizadaEvent> context)
    {
        var mensaje = context.Message;

        _logger.LogInformation($"🏁 Recibido evento: Carrera {mensaje.IdCarrera} ha FINALIZADO");

        try
        {
            // 1️⃣ Actualizar estado en la base de datos
            var carrera = await _carreraRepository.GetByIdAsync(mensaje.IdCarrera);

            if (carrera == null)
            {
                _logger.LogWarning($"⚠️ Carrera {mensaje.IdCarrera} no encontrada en BD");
                return;
            }

            _logger.LogInformation($"📊 Carrera {mensaje.IdCarrera} - Estado actual: {carrera.EstadoCarrera}");

            carrera.EstadoCarrera = Carrera.Estado.Finalizada;
            carrera.FechaFin = mensaje.FechaFin ?? DateTime.UtcNow;

            await _carreraRepository.UpdateAsync(carrera);

            _logger.LogInformation($"✅ Estado actualizado a FINALIZADA para carrera {mensaje.IdCarrera}");

            // 2️⃣ Notificar a los clientes conectados por SignalR
            await _hubContext.Clients.Group($"carrera-{mensaje.IdCarrera}")
                .SendAsync("CarreraFinalizada", new
                {
                    carreraId = mensaje.IdCarrera,
                    fechaFin = carrera.FechaFin,
                    mensaje = "La carrera ha finalizado"
                });

            _logger.LogInformation($"📢 Notificación de finalización enviada a clientes SignalR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al procesar finalización de carrera {mensaje.IdCarrera}");
            throw; // Para que MassTransit reintente
        }
    }
}

// ✅ Evento que debe publicar el simulador cuando TODOS los corredores terminen
