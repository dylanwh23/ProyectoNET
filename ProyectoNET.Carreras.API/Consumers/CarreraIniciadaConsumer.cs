using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared.WebApp; // O donde estén tus DTOs
using ProyectoNET.Carreras.API.Services;

namespace ProyectoNET.Carreras.API.Consumers
{
    public class CarreraIniciadaConsumer : IConsumer<CarreraIniciadaEvent>
    {
        private readonly ICarreraStateService _stateService;
        private readonly IHubContext<CarreraHub> _hubContext; // 1. Re-inyectar el Hub
        private readonly ILogger<CarreraIniciadaConsumer> _logger;

        public CarreraIniciadaConsumer(
            ICarreraStateService stateService, 
            IHubContext<CarreraHub> hubContext, // 2. Re-inyectar el Hub
            ILogger<CarreraIniciadaConsumer> logger)
        {
            _stateService = stateService;
            _hubContext = hubContext; // 3. Re-inyectar el Hub
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CarreraIniciadaEvent> context)
        {
            var evento = context.Message;
            _logger.LogInformation("API recibió CarreraIniciadaEvent para Carrera ID: {Id}", evento.IdCarrera);
            
            // 4. Tarea 1: Actualizar la caché (esto estaba bien)
            _stateService.InicializarCarrera(evento);
            
            // 5. Tarea 2: ¡ARREGLO! Notificar a los clientes "en vivo"
            string grupoSignalR = $"carrera-{evento.IdCarrera}";
            
            await _hubContext.Clients.Group(grupoSignalR)
                .SendAsync(
                    "CarreraIniciada",           // El mensaje que Blazor espera
                    evento.IdCorredores,
                    evento.TotalPuntosDeControl
                );
                
            _logger.LogInformation("Evento 'CarreraIniciada' retransmitido a SignalR (Live Start)");
        }
    }
}