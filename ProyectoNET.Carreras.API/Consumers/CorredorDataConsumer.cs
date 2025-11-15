using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared;

namespace ProyectoNET.Carreras.API.Consumers;

// TiempoRegistradoConsumer.cs
public class CorredorDataConsumer : IConsumer<Shared.EventosRabbit.CorredorData>
{
    private readonly ILogger<CorredorDataConsumer> _logger;
    public CorredorDataConsumer( ILogger<CorredorDataConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<Shared.EventosRabbit.CorredorData> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("📥 Progreso recibido - Corredor {IdCorredor}", mensaje.IdCorredor);
        return Task.CompletedTask;
    }
}