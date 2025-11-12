using MassTransit;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;

namespace ProyectoNET.SimulatorWorker.Consumers;

public class IniciarCarreraConsumer(
    ILogger<IniciarCarreraConsumer> logger,
    IBus bus) 
    : IConsumer<IniciarCarreraCommand>
{
    private readonly Random _random = new();
    
    public async Task Consume(ConsumeContext<IniciarCarreraCommand> context)
    {
        var command = context.Message;
        logger.LogInformation("▶️ Iniciando simulación para la carrera {IdCarrera}.", command.IdCarrera);
        
        // 1. Simular la carrera completa primero
        var simulacionCompleta = SimularCarreraCompleta(command);

        logger.LogInformation("✅ Simulación para la carrera {IdCarrera} finalizada. Enviando eventos", command.IdCarrera);
        
        
        var eventoInicio = new CarreraIniciadaEvent(
            command.IdCarrera,
            command.IdCorredores,
            command.TotalPuntosDeControl
        );
        
        // Publicamos el evento
        await bus.Publish(eventoInicio);

        // 2. Enviar eventos en background
        _ = Task.Run(async () =>
        {
            try
            {
                await EnviarEventosEnTiempoReal(command.IdCarrera, simulacionCompleta);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Error al enviar eventos para la carrera {IdCarrera}", command.IdCarrera);
            }
        });
        
        logger.LogInformation("🚀 Simulación de carrera {IdCarrera} iniciada en background", command.IdCarrera);
        await Task.CompletedTask;
    }
    
    private Dictionary<int, List<EventoCorredor>> SimularCarreraCompleta(IniciarCarreraCommand command)
    {
        var eventosPorCorredor = new Dictionary<int, List<EventoCorredor>>();
        
        foreach (var idCorredor in command.IdCorredores)
        {
            var eventos = new List<EventoCorredor>();
            var tiemposPorTramo = new List<TiempoPorTramoDTO>();
            var tiempoAcumulado = TimeSpan.Zero;
            
            var ritmoBaseMinPorKm = 4.0 + _random.NextDouble() * 4.0;
            
            for (int i = 0; i < command.TotalPuntosDeControl.Count; i++)
            {
                var puntoActual = command.TotalPuntosDeControl[i];
                
                // ... (tu lógica de distanciaTramo, ritmo, fatiga, etc.) ...
                float distanciaTramo;
                if (i == 0) { distanciaTramo = puntoActual.Km; }
                else { distanciaTramo = puntoActual.Km - command.TotalPuntosDeControl[i - 1].Km; }
                
                var variacion = 0.75 + _random.NextDouble() * 0.5;
                var ritmoTramoMinPorKm = ritmoBaseMinPorKm * variacion;
                var factorFatiga = 1.0 + (i * 0.02);
                ritmoTramoMinPorKm *= factorFatiga;
                
                var tiempoTramo = TimeSpan.FromMinutes(distanciaTramo * ritmoTramoMinPorKm);
                tiempoAcumulado += tiempoTramo;
                var velocidadKmh = (float)(distanciaTramo / tiempoTramo.TotalHours);
                
                var desdePuntoId = i == 0 ? 0 : command.TotalPuntosDeControl[i - 1].IdPuntoDeControl;
                tiemposPorTramo.Add(new TiempoPorTramoDTO(
                    desdePuntoId,
                    puntoActual.IdPuntoDeControl,
                    tiempoTramo
                ));
                
                // Crear evento para este checkpoint
                eventos.Add(new EventoCorredor
                {
                    IdCorredor = idCorredor,
                    TiempoReal = tiempoAcumulado,
                    Checkpoint = puntoActual,
                    KmRecorridos = puntoActual.Km, // <-- ¡AÑADIDO! (1 de 3)
                    VelocidadKmh = velocidadKmh, 
                    TiemposPorTramo = new List<TiempoPorTramoDTO>(tiemposPorTramo)
                });
            }
            
            eventosPorCorredor[idCorredor] = eventos;
        }
        
        return eventosPorCorredor;
    }
    
    private async Task EnviarEventosEnTiempoReal(int idCarrera, Dictionary<int, List<EventoCorredor>> simulacion)
    {
        // ... (tu lógica de ordenar eventos y calcular tiempo) ...
        var todosLosEventos = simulacion
            .SelectMany(kvp => kvp.Value.Select(e => new { Evento = e, IdCorredor = kvp.Key }))
            .OrderBy(x => x.Evento.TiempoReal)
            .ToList();
        
        var tiempoInicio = DateTime.UtcNow;
        var factorAceleracion = 20.0; 
        
        foreach (var item in todosLosEventos)
        {
            var tiempoSimuladoEnSegundos = item.Evento.TiempoReal.TotalSeconds;
            var tiempoRealEnSegundos = tiempoSimuladoEnSegundos / factorAceleracion;
            var tiempoObjetivo = tiempoInicio.AddSeconds(tiempoRealEnSegundos);
            
            var esperaMs = (int)(tiempoObjetivo - DateTime.UtcNow).TotalMilliseconds;
            if (esperaMs > 0)
            {
                await Task.Delay(esperaMs);
            }
            
            // Publicar evento
            var evento = new ProgresoCorredorActualizado(
                idCarrera,
                item.IdCorredor,
                item.Evento.Checkpoint,
                item.Evento.KmRecorridos, // <-- ¡AÑADIDO! (2 de 3)
                item.Evento.VelocidadKmh,
                item.Evento.TiemposPorTramo
            );
            
            await bus.Publish(evento);
            
            logger.LogInformation(
                "📍 Corredor {IdCorredor} pasó por checkpoint {Km}km a {Velocidad:F2} km/h (tiempo: {Tiempo})",
                item.IdCorredor,
                item.Evento.Checkpoint.Km,
                item.Evento.VelocidadKmh,
                item.Evento.TiempoReal.ToString(@"hh\:mm\:ss")
            );
        }
        
        logger.LogInformation("🏁 Todos los eventos de la carrera {IdCarrera} han sido enviados.", idCarrera);
    }
    
    // Clase auxiliar para manejar eventos internamente
    private class EventoCorredor
    {
        public int IdCorredor { get; set; }
        public TimeSpan TiempoReal { get; set; }
        public PuntosDeControlDTO Checkpoint { get; set; } = null!;
        public float KmRecorridos { get; set; } // <-- ¡AÑADIDO! (3 de 3)
        public float VelocidadKmh { get; set; }
        public List<TiempoPorTramoDTO> TiemposPorTramo { get; set; } = new();
    }
}