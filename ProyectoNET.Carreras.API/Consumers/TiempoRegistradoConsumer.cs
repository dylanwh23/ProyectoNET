using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared;
using ProyectoNET.Carreras.API.Services;

namespace ProyectoNET.Carreras.API.Consumers;

// TiempoRegistradoConsumer.cs
public class TiempoRegistradoConsumer : IConsumer<ProgresoCorredorActualizado>
{
    private readonly ICarreraStateService _stateService; // 1. Inyectar caché
    private readonly ILogger<TiempoRegistradoConsumer> _logger;
    // (Adiós IHubContext)

    public TiempoRegistradoConsumer(ICarreraStateService stateService, ILogger<TiempoRegistradoConsumer> logger)
    {
        _stateService = stateService;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProgresoCorredorActualizado> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("📥 Progreso recibido - Corredor {IdCorredor}", mensaje.IdCorredor);
        
        // 2. SOLO actualiza la caché. No envíes SignalR.
        _stateService.ActualizarProgreso(mensaje);
        
        return Task.CompletedTask;
    }
}