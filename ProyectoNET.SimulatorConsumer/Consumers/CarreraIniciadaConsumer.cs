using MassTransit;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;
using ProyectoNET.Shared.EventosRabbit;
using ProyectoNET.WebApp.Hubs;

namespace ProyectoNET.SimulatorConsumer.Consumers;

public class CarreraIniciadaConsumer : IConsumer<CarreraIniciada>
{
    private readonly ILogger<CarreraIniciadaConsumer> _logger;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IHubContext<CarrerasHub> _hubContext;

    public CarreraIniciadaConsumer(
        ILogger<CarreraIniciadaConsumer> logger,
        IConnectionMultiplexer redisConnection,
        IHubContext<CarrerasHub> hubContext)
    {
        _logger = logger;
        _redisConnection = redisConnection;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CarreraIniciada> context)
    {
        var mensaje = context.Message;
        _logger.LogInformation("Procesando INICIO DE MARATON {Id}. Cargando {Cnt} corredores...", mensaje.IdCarrera, mensaje.IdCorredores.Count);

        try
        {
            var db = _redisConnection.GetDatabase();
            string raceKey = $"race:{mensaje.IdCarrera}";
            var hashEntries = mensaje.IdCorredores
                .Select(c => new HashEntry(
                    $"runner:{c.IdCorredor}", 
                    JsonSerializer.Serialize(c)
                ))
                .ToArray();

            await db.HashSetAsync(raceKey, hashEntries);
            await _hubContext.Clients.Group(mensaje.IdCarrera.ToString()).SendAsync("ReceiveCurrentState", mensaje.IdCorredores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar la carrera {Id}", mensaje.IdCarrera);
            throw;
        }
    }
}