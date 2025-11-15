// ProyectoNET.Carreras.API/Sagas/CarreraSaga.cs

using MassTransit;
using Microsoft.AspNetCore.SignalR;
using ProyectoNET.Carreras.API.Hubs;
using ProyectoNET.Shared.EventosRabbit;
using System.Security.Cryptography; // Para el hash
using System.Text;
using StackExchange.Redis;
using System.Text.Json;

namespace ProyectoNET.Carreras.API.Sagas;

public class CarreraSaga : MassTransitStateMachine<CarreraSagaState>
{
    private readonly IHubContext<CarreraHub> _hubContext;

    // --- Un "espacio de nombres" (namespace) para nuestros Guids de carrera ---
    // (Este Guid es aleatorio, solo tiene que ser estático y único para este propósito)
    private static readonly Guid CarreraNamespaceGuid = 
        new("f9c1c6a1-3e28-4e89-9ea2-66916c0d603e");

    public State Iniciada { get; private set; } = null!;
    public Event<CarreraIniciada> CarreraIniciadaEvent { get; private set; } = null!;
    public Event<CorredorData> CorredorDataEvent { get; private set; } = null!;
    public Event<CarreraFinalizadaEvent> CarreraFinalizadaEvent { get; private set; } = null!;
    private readonly IConnectionMultiplexer _redis;
 

    public CarreraSaga(IHubContext<CarreraHub> hubContext, ILogger<CarreraSaga> logger, IConnectionMultiplexer redis)
    {
        _hubContext = hubContext;
        _redis = redis;
        InstanceState(x => x.CurrentState);

        // --- A. Correlación por Guid Determinístico ---
        // ¡Este es el cambio clave!
        
       // --- A. Correlación (Se mantiene igual, por Guid determinístico) ---
        Event(() => CarreraIniciadaEvent, x => 
            x.CorrelateById(context => GetDeterministicGuid(context.Message.IdCarrera)));
        Event(() => CorredorDataEvent, x => 
            x.CorrelateById(context => GetDeterministicGuid(context.Message.IdCarrera)));
        Event(() => CarreraFinalizadaEvent, x => 
            x.CorrelateById(context => GetDeterministicGuid(context.Message.IdCarrera)));

        // --- B. Lógica del Flujo ---

        Initially(
            When(CarreraIniciadaEvent)
                .Then(context =>
                {
                    // Solo guardamos el estado base
                    context.Saga.IdCarrera = context.Message.IdCarrera;
                    context.Saga.FechaInicio = DateTime.UtcNow;
                })
                .TransitionTo(Iniciada)
        );

        // --- C. ¡AQUÍ ESTÁ EL CAMBIO! ---
        During(Iniciada,
            // (Volvemos a .ThenAsync porque la escritura a Redis es asíncrona)
            When(CorredorDataEvent)
                .ThenAsync(async context =>
                {
                    var mensaje = context.Message;
                    var estado = new CorredorEstadoActual
                    {
                        IdCorredor = mensaje.IdCorredor,
                        UltimoPuntoDeControlId = mensaje.UltimoCheckpoint,
                        VelocidadKmh = mensaje.Velocidad
                    };

                    // 1. Definir claves de Redis
                    string hashKey = $"carrera:estado:{mensaje.IdCarrera}";
                    string hashField = $"corredor:{mensaje.IdCorredor}";
                    
                    // 2. Serializar SÓLO este corredor
                    string jsonValue = JsonSerializer.Serialize(estado);

                    // 3. Escribir en el Hash de Redis (Operación O(1))
                    var db = _redis.GetDatabase();
                    await db.HashSetAsync(hashKey, hashField, jsonValue);

                    /*
                    // 4. Fire-and-Forget a SignalR (como lo teníamos)
                    _ = _hubContext.Clients.Group($"carrera-{mensaje.IdCarrera}")
                        .SendAsync("CorredorActualizado", estado);
                        */

                    logger.LogInformation("escrito en redis");    
                })
        );

        During(Iniciada,
            When(CarreraFinalizadaEvent)
                .ThenAsync(async context =>
                {
                    // Cuando la carrera termina, debemos borrar el Hash
                    string hashKey = $"carrera:estado:{context.Message.IdCarrera}";
                    var db = _redis.GetDatabase();
                    await db.KeyDeleteAsync(hashKey);
                    
                    // (Tu lógica de SignalR para finalizar, etc.)
                })
                .Finalize() // Borra la Saga (el estado mínimo) de Redis
        );

        SetCompletedWhenFinalized();
    }

    /// <summary>
    /// Genera un Guid V5 (basado en hash) determinístico a partir de un int.
    /// Esto asegura que "IdCarrera = 123" SIEMPRE genere el mismo Guid.
    /// </summary>
    private static Guid GetDeterministicGuid(int idCarrera)
    {
        // Usamos el IdCarrera como "nombre" y nuestro Guid estático como "namespace"
        var nameBytes = Encoding.UTF8.GetBytes(idCarrera.ToString());
        var namespaceBytes = CarreraNamespaceGuid.ToByteArray();
        
        // Combina el namespace y el nombre
        var data = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, data, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, data, namespaceBytes.Length, nameBytes.Length);

        // Calcula el hash (SHA1)
        byte[] hash;
        using (var sha1 = SHA1.Create())
        {
            hash = sha1.ComputeHash(data);
        }

        // Crea el Guid V5 a partir del hash
        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);
        
        // Establece la versión a 5
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));
        // Establece la variante
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        return new Guid(newGuid);
    }
}