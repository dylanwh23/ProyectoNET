using MassTransit;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;
using ProyectoNET.Shared.EventosRabbit;
using ProyectoNET.WebApp.Hubs;
namespace ProyectoNET.SimulatorConsumer.Consumers;
public class CorredorDataConsumer : IConsumer<CorredorData>
{
    private readonly ILogger<CorredorDataConsumer> _logger;
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IHubContext<CarrerasHub> _hubContext;

    public CorredorDataConsumer(
      ILogger<CorredorDataConsumer> logger,
      IConnectionMultiplexer redisConnection,
      IHubContext<CarrerasHub> hubContext)
    {
        _logger = logger;
        _redisConnection = redisConnection;
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CorredorData> context)
    {
        var raceEvent = context.Message;

        try
        {
            await SaveStateToRedis(raceEvent);
            await _hubContext.Clients.Group(raceEvent.IdCarrera.ToString()).SendAsync("ReceiveRaceUpdate", raceEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando evento de carrera");
            throw;
        }
    }

    private async Task SaveStateToRedis(CorredorData raceEvent)
    {
        var db = _redisConnection.GetDatabase();

        string raceKey = $"race:{raceEvent.IdCarrera}";
        string runnerField = $"runner:{raceEvent.IdCorredor}";
        string eventJson = JsonSerializer.Serialize(raceEvent);

        await db.HashSetAsync(raceKey, runnerField, eventJson);
    }
}