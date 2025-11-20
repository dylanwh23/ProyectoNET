using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using ProyectoNET.Shared.EventosRabbit;
using StackExchange.Redis;

namespace ProyectoNET.WebApp.Hubs;

public class CarrerasHub : Hub
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly ILogger<CarrerasHub> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CarrerasHub(IConnectionMultiplexer redisConnection, ILogger<CarrerasHub> logger)
    {
        _redisConnection = redisConnection;
        _logger = logger;
    }

    public async Task JoinRaceGroup(string raceId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, raceId);
        
        var db = _redisConnection.GetDatabase();
        var raceKey = $"race:{raceId}";
        var hashEntries = await db.HashGetAllAsync(raceKey);

        if (hashEntries.Length > 0)
        {
            var allRunners = new List<CorredorData>();
            foreach (var entry in hashEntries)
            {
                if (!entry.Value.HasValue) continue;
                var corredor = JsonSerializer.Deserialize<CorredorData>(entry.Value.ToString(), _jsonOptions);
                if (corredor != null) allRunners.Add(corredor);
            }

            // CARGA INICIAL: Usamos lotes porque son muchos datos y no queremos bloquear la red
            const int BatchSize = 500; 
            await Clients.Client(Context.ConnectionId).SendAsync("ResetLocalState");
            
            foreach (var batch in allRunners.Chunk(BatchSize))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveRaceUpdateBatch", batch);
                await Task.Delay(20); 
            }
        }
    }

    public async Task LeaveRaceGroup(string raceId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, raceId);
    }
    
    // NOTA: Los consumidores llaman directamente a Clients.Group(...).SendAsync("ReceiveRaceUpdate", ...)
    // No hay método aquí para eso porque se invoca desde fuera (CorredorDataConsumer).
    // Asegúrate de que CorredorDataConsumer NO use métodos de batching propios, que envíe uno a uno.
}