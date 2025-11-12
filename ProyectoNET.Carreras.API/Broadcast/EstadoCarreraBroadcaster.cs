using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Carreras.API.Services;

// Este servicio se ejecuta en el fondo de la API
public class EstadoCarreraBroadcaster : BackgroundService
{
    private readonly ICarreraStateService _stateService;
    private readonly IHubContext<CarreraHub> _hubContext;
    private readonly TimeSpan _periodo = TimeSpan.FromSeconds(1); // ¡Controla la "metralleta" aquí!

    public EstadoCarreraBroadcaster(ICarreraStateService stateService, IHubContext<CarreraHub> hubContext)
    {
        _stateService = stateService;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Espera 1 segundo
            await Task.Delay(_periodo, stoppingToken);

            // 1. Obtiene todas las carreras activas (las que están en la caché)
            var carrerasActivas = _stateService.GetCarrerasActivas();

            foreach (var idCarrera in carrerasActivas)
            {
                // 2. Obtiene el estado COMPLETO de esa carrera
                var estadoActual = _stateService.GetEstadoCorredores(idCarrera);
                
                // 3. Envía el estado completo al grupo de SignalR
                //    (Tu Blazor ya tiene el listener "EstadoCompletoRecibido")
                await _hubContext.Clients
                    .Group($"carrera-{idCarrera}")
                    .SendAsync("EstadoCompletoRecibido", estadoActual, stoppingToken);
            }
        }
    }
}