
using ProyectoNET.WebApp.Hubs;
using ProyectoNET.Shared.EventosRabbit;  
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;


namespace ProyectoNET.SimulatorConsumer.Consumers;

    public class EventosCorredorConsumer : BackgroundService
    {
        private readonly ILogger<EventosCorredorConsumer> _logger;
        private readonly IConnection _rabbitConnection;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IHubContext<CarrerasHub> _hubContext;
        
        // CAMBIO 1: IModel ya no existe, ahora es IChannel
        private IChannel _channel; 

        public EventosCorredorConsumer(
            ILogger<EventosCorredorConsumer> logger,
            IConnection rabbitConnection,
            IConnectionMultiplexer redisConnection,
            IHubContext<CarrerasHub> hubContext)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection; // Aspire inyecta la conexión
            _redisConnection = redisConnection;
            _hubContext = hubContext;
        }

       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    try 
    {
        // 1. Crear el canal (RabbitMQ Client v7+)
        _channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

        // -------------------------------------------------------------------------
        // CONFIGURACIÓN DE TOPOLOGÍA (PUENTE MASSTRANSIT -> CLIENTE NATIVO)
        // -------------------------------------------------------------------------
        
        // Nombre de la cola donde este servicio escuchará
        string queueName = "race-events";

        // Nombre del Exchange donde MassTransit publica por defecto.
        // Formato obligatorio: "Namespace:NombreClase"
        // Basado en tu código anterior: namespace ProyectoNET.Shared.EventosRabbit, clase CorredorData
        string massTransitExchangeName = "ProyectoNET.Shared.EventosRabbit:CorredorData";

        // PASO A: Declarar la Cola (destino final)
        await _channel.QueueDeclareAsync(
            queue: queueName, 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null,
            cancellationToken: stoppingToken);

        // PASO B: Declarar el Exchange (origen MassTransit)
        // MassTransit usa 'Fanout' y 'Durable' por defecto para eventos
        await _channel.ExchangeDeclareAsync(
            exchange: massTransitExchangeName, 
            type: ExchangeType.Fanout, 
            durable: true, 
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        // PASO C: Crear el Binding (El puente)
        // Esto dice: "Lo que llegue al exchange de MassTransit, envíalo a mi cola 'race-events'"
        await _channel.QueueBindAsync(
            queue: queueName, 
            exchange: massTransitExchangeName, 
            routingKey: "", 
            arguments: null,
            cancellationToken: stoppingToken);

        // -------------------------------------------------------------------------
        // CONFIGURACIÓN DEL CONSUMIDOR
        // -------------------------------------------------------------------------

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                // Asegúrate de usar el tipo correcto (CorredorData)
                var raceEvent = JsonSerializer.Deserialize<CorredorData>(message);
                
                if (raceEvent != null)
                {
                    // 1. Guardar en Redis (Persistencia de estado)
                    await SaveStateToRedis(raceEvent);

                    // 2. Publicar en SignalR Backplane (Redis Pub/Sub)
                    // El servicio WebBlazor escuchará esto y actualizará la UI
                    await _hubContext.Clients.Group(raceEvent.IdCarrera.ToString())
                            .SendAsync("ReceiveRaceUpdate", raceEvent, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando mensaje: {Msg}", message);
            }
        };

        // Iniciar el consumo de la cola "race-events"
        await _channel.BasicConsumeAsync(
            queue: queueName, 
            autoAck: true, 
            consumer: consumer,
            cancellationToken: stoppingToken);
        
        _logger.LogInformation("✅ Consumer nativo escuchando cola '{Queue}' conectada a '{Exchange}'", queueName, massTransitExchangeName);

        // Mantener el servicio vivo
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    catch (Exception ex)
    {
        _logger.LogCritical(ex, "❌ Error fatal iniciando el consumidor RabbitMQ");
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

        public override void Dispose()
        {
            _channel?.Dispose();
            base.Dispose();
        }
    }
