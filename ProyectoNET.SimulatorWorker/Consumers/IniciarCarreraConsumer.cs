using MassTransit;
using ProyectoNET.Shared.EventosRabbit;

namespace ProyectoNET.SimulatorWorker.Consumers;

public class IniciarCarreraConsumer(
    ILogger<IniciarCarreraConsumer> logger,
    IBus bus) 
    : IConsumer<IniciarCarreraCommand>
{
    private readonly Random _random = new();

    // ✅ NUEVO: Configuración de salidas tipo maratón
    private const int CORREDORES_POR_OLEADA = 50; // Grupos grandes de ~50 corredores
    private const int INTERVALO_ENTRE_OLEADAS_SEGUNDOS = 30; // 30 segundos entre oleadas
    private const int VARIACION_SALIDA_DENTRO_OLEADA_MAX_SEGUNDOS = 5; // Variación aleatoria dentro de la oleada

    public async Task Consume(ConsumeContext<IniciarCarreraCommand> context)
    {
        var command = context.Message;
        logger.LogInformation("▶️ Iniciando simulación para la carrera {IdCarrera}.", command.IdCarrera);

        // 1. Simular la carrera completa con salidas escalonadas
        var simulacionCompleta = SimularCarreraCompleta(command);

        logger.LogInformation("✅ Simulación para la carrera {IdCarrera} finalizada. Enviando eventos", command.IdCarrera);
        
        var eventoInicio = new CarreraIniciada(
            command.IdCarrera,
            command.IdCorredores,
            command.TotalPuntosDeControl
        );
        
        await bus.Publish(eventoInicio);

        // 2. Enviar eventos en background
        _ = Task.Run(async () =>
        {
            try
            {
                await EnviarEventosEnTiempoReal(command.IdCarrera, simulacionCompleta, command.IdCorredores.Count);
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

        // ✅ NUEVO: Calcular tiempo de salida para cada corredor
        var tiemposSalida = CalcularTiemposDeSalida(command.IdCorredores);

        foreach (var idCorredor in command.IdCorredores)
        {
            var eventos = new List<EventoCorredor>();
            var tiemposPorTramo = new List<TiempoPorTramoDTO>();
            
            // ✅ MODIFICADO: Tiempo para llegar al checkpoint 1 (desde que se da el pistoletazo)
            var tiempoHastaCheckpoint1 = tiemposSalida[idCorredor];
            
            // ✅ NUEVO: Tiempo oficial del corredor (desde que cruza checkpoint 1)
            var tiempoOficial = TimeSpan.Zero;

            // Generar ritmo base del corredor (minutos por km, entre 4 y 8 min/km)
            var ritmoBaseMinPorKm = 4.0 + _random.NextDouble() * 4.0;

            for (int i = 0; i < command.TotalPuntosDeControl.Count; i++)
            {
                var puntoActual = command.TotalPuntosDeControl[i];

                // Calcular distancia del tramo
                float distanciaTramo;
                if (i == 0)
                {
                    distanciaTramo = puntoActual.Km;
                }
                else
                {
                    var puntoAnterior = command.TotalPuntosDeControl[i - 1];
                    distanciaTramo = puntoActual.Km - puntoAnterior.Km;
                }

                // Variar el ritmo del corredor
                var variacion = 0.75 + _random.NextDouble() * 0.5;
                var ritmoTramoMinPorKm = ritmoBaseMinPorKm * variacion;

                // Simular fatiga progresiva
                var factorFatiga = 1.0 + (i * 0.02);
                ritmoTramoMinPorKm *= factorFatiga;

                // Calcular TIEMPO del tramo
                var tiempoTramo = TimeSpan.FromMinutes(distanciaTramo * ritmoTramoMinPorKm);
                
                // ✅ MODIFICADO: Actualizar tiempos según el checkpoint
                if (i == 0)
                {
                    // Checkpoint 1: sumar el tiempo de salida
                    tiempoHastaCheckpoint1 += tiempoTramo;
                    tiempoOficial = TimeSpan.Zero; // El tiempo oficial aún no comienza
                }
                else
                {
                    // Después del checkpoint 1: acumular tiempo oficial
                    tiempoOficial += tiempoTramo;
                }

                // Calcular VELOCIDAD
                var velocidadKmh = (float)(distanciaTramo / tiempoTramo.TotalHours);

                // Agregar tiempo del tramo
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
                    TiempoReal = tiempoHastaCheckpoint1 + tiempoOficial, // Tiempo absoluto desde el pistoletazo
                    TiempoOficial = i == 0 ? TimeSpan.Zero : tiempoOficial, // ✅ NUEVO: Tiempo desde checkpoint 1
                    TiempoSalida = tiemposSalida[idCorredor],
                    Checkpoint = puntoActual,
                    VelocidadKmh = velocidadKmh, 
                    TiemposPorTramo = new List<TiempoPorTramoDTO>(tiemposPorTramo)
                });
            }

            eventosPorCorredor[idCorredor] = eventos;
        }

        return eventosPorCorredor;
    }

    // ✅ NUEVO: Método para calcular tiempos de salida tipo maratón
    private Dictionary<int, TimeSpan> CalcularTiemposDeSalida(List<int> idCorredores)
    {
        var tiemposSalida = new Dictionary<int, TimeSpan>();
        var tiempoBaseOleada = TimeSpan.Zero;
        var numeroOleada = 1;

        for (int i = 0; i < idCorredores.Count; i++)
        {
            // Nueva oleada cada X corredores
            if (i > 0 && i % CORREDORES_POR_OLEADA == 0)
            {
                tiempoBaseOleada += TimeSpan.FromSeconds(INTERVALO_ENTRE_OLEADAS_SEGUNDOS);
                numeroOleada++;
                logger.LogInformation("🌊 Oleada {NumOleada} sale en: {Tiempo}", 
                    numeroOleada, 
                    tiempoBaseOleada.ToString(@"mm\:ss"));
            }

            // Dentro de cada oleada, los corredores salen casi simultáneamente
            // pero con pequeñas variaciones (0-5 segundos) para simular posiciones en la línea de salida
            var variacionDentroOleada = TimeSpan.FromSeconds(_random.NextDouble() * VARIACION_SALIDA_DENTRO_OLEADA_MAX_SEGUNDOS);
            tiemposSalida[idCorredores[i]] = tiempoBaseOleada + variacionDentroOleada;
        }

        logger.LogInformation("✅ Total de {TotalCorredores} corredores distribuidos en {NumOleadas} oleadas", 
            idCorredores.Count, 
            numeroOleada);

        return tiemposSalida;
    }

    private async Task EnviarEventosEnTiempoReal(
        int idCarrera,
        Dictionary<int, List<EventoCorredor>> simulacion,
        int totalCorredores)
    {
        // Ordenar eventos por tiempo real (que ahora incluye el tiempo de salida)
        var todosLosEventos = simulacion
            .SelectMany(kvp => kvp.Value.Select(e => new { Evento = e, IdCorredor = kvp.Key }))
            .OrderBy(x => x.Evento.TiempoReal)
            .ToList();

        var tiempoInicio = DateTime.UtcNow;
        var factorAceleracion = 5;

        foreach (var item in todosLosEventos)
        {
            // Calcular cuánto tiempo debe esperar
            var tiempoSimuladoEnSegundos = item.Evento.TiempoReal.TotalSeconds;
            var tiempoRealEnSegundos = tiempoSimuladoEnSegundos / factorAceleracion;
            var tiempoObjetivo = tiempoInicio.AddSeconds(tiempoRealEnSegundos);

            // Esperar hasta el momento correcto
            var esperaMs = (int)(tiempoObjetivo - DateTime.UtcNow).TotalMilliseconds;
            if (esperaMs > 0)
            {
                await Task.Delay(esperaMs);
            }

            // Publicar evento de progreso
            var evento = new CorredorData(
                idCarrera,
                item.IdCorredor,
                item.Evento.VelocidadKmh,
                item.Evento.Checkpoint.IdPuntoDeControl
            );

            await bus.Publish(evento);

            // ✅ MODIFICADO: Mostrar tiempo oficial (desde checkpoint 1)
            logger.LogInformation(
                "📍 Corredor {IdCorredor} pasó por checkpoint {Km}km a {Velocidad:F2} km/h (tiempo oficial: {TiempoOficial} | oleada: {TiempoSalida})",
                item.IdCorredor,
                item.Evento.Checkpoint.Km,
                item.Evento.VelocidadKmh,
                item.Evento.TiempoOficial.ToString(@"hh\:mm\:ss"),
                item.Evento.TiempoSalida.ToString(@"mm\:ss")
            );
        }

        logger.LogInformation("🏁 Todos los eventos de la carrera {IdCarrera} han sido enviados.", idCarrera);

        // Publicar evento de finalización de carrera
        var eventoFinalizacion = new CarreraFinalizadaEvent(
            idCarrera,
            DateTime.UtcNow,
            totalCorredores,
            simulacion.Count
        );

        await bus.Publish(eventoFinalizacion);

        logger.LogInformation(
            "🎉 CARRERA {IdCarrera} FINALIZADA - Evento de finalización publicado ({Corredores} corredores)",
            idCarrera,
            totalCorredores
        );
    }

    // ✅ MODIFICADO: Clase auxiliar con TiempoOficial
    private class EventoCorredor
    {
        public int IdCorredor { get; set; }
        public TimeSpan TiempoReal { get; set; } // Tiempo absoluto desde el pistoletazo (para ordenar eventos)
        public TimeSpan TiempoOficial { get; set; } // ✅ NUEVO: Tiempo oficial desde checkpoint 1 (para el podio)
        public TimeSpan TiempoSalida { get; set; } // Momento en que salió su oleada
        public PuntosDeControlDTO Checkpoint { get; set; } = null!;
        public double VelocidadKmh { get; set; }
        public List<TiempoPorTramoDTO> TiemposPorTramo { get; set; } = new();
    }
    
    public record TiempoPorTramoDTO(int DesdePuntoDeControlId, int HastaPuntoDeControlId, TimeSpan Tiempo);
}