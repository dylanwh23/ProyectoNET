using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using ProyectoNET.Shared.EventosRabbit;
using StackExchange.Redis;

namespace ProyectoNET.WebApp.Hubs;

    public class CarrerasHub : Hub
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<CarrerasHub> _logger;

        public CarrerasHub(IConnectionMultiplexer redisConnection, ILogger<CarrerasHub> logger)
        {
            _redisConnection = redisConnection;
            _logger = logger;
        }

        // Cliente Blazor llama a esto al entrar a la página
        public async Task JoinRaceGroup(string raceId)
        {
            // 1. Unir al cliente al grupo de SignalR
            await Groups.AddToGroupAsync(Context.ConnectionId, raceId);
            _logger.LogInformation("Cliente {Id} unido al grupo {RaceId}", Context.ConnectionId, raceId);

            // 2. Obtener el estado actual de la carrera desde Redis
            var db = _redisConnection.GetDatabase();
            var raceKey = $"race:{raceId}";
            var hashEntries = await db.HashGetAllAsync(raceKey);

            if (hashEntries.Length > 0)
            {
                var currentState = hashEntries
                    .Select(entry => JsonSerializer.Deserialize<CorredorData>(entry.Value))
                    .ToList();

                // 3. Enviar el estado actual SOLO al cliente que acaba de conectarse
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveCurrentState", currentState);
            }
        }

        public async Task LeaveRaceGroup(string raceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, raceId);
        }
    }
