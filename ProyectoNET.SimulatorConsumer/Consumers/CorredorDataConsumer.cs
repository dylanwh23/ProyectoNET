using MassTransit;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text.Json;
using ProyectoNET.Shared.EventosRabbit; // Tu namespace de eventos
using ProyectoNET.WebApp.Hubs;
namespace ProyectoNET.SimulatorConsumer.Consumers
{
    // IConsumer<T> es la interfaz mágica de MassTransit
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
            var raceEvent = context.Message; // ¡Ya viene deserializado!

            try
            {
                // 1. Guardar en Redis
                await SaveStateToRedis(raceEvent);

                // 2. Enviar a SignalR (Backplane)
                await _hubContext.Clients.Group(raceEvent.IdCarrera.ToString())
                     .SendAsync("ReceiveRaceUpdate", raceEvent);
                     
                // Log opcional (para ver que fluye)
                _logger.LogInformation("Evento procesado:Carrera{IdCarrera} - Corredor {IdCorredor} - CheckPoint {checkpoint} - km {Km}", raceEvent.IdCarrera, raceEvent.IdCorredor, raceEvent.UltimoCheckpoint, raceEvent.Velocidad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando evento de carrera");
                // Si lanzas la excepción, MassTransit reintentará automáticamente
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
}