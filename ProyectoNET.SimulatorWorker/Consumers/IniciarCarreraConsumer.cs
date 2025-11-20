using MassTransit;
using ProyectoNET.Shared.EventosRabbit;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Features;

namespace ProyectoNET.SimulatorWorker.Consumers;

public class IniciarCarreraConsumer(
    ILogger<IniciarCarreraConsumer> logger,
    IBus bus)
    : IConsumer<IniciarCarreraCommand>
{
    private readonly Random _random = new();

    //constantes de configuracion
    private const int CORREDORES_POR_OLEADA = 50;
    private const int INTERVALO_ENTRE_OLEADAS_SEGUNDOS = 30;
    private const int VARIACION_SALIDA_DENTRO_OLEADA_MAX_SEGUNDOS = 5;

    public async Task Consume(ConsumeContext<IniciarCarreraCommand> context)
    {
        var command = context.Message;

        //validación de recibir geojson
        if (string.IsNullOrEmpty(command.RutaGeoJson))
        {
            logger.LogError("No se puede iniciar la simulación para la maratón {IdCarrera}: Falta la Ruta GeoJSON.", command.IdCarrera);
            return;
        }
        //validación de recibir checkpoints
        if (command.TotalPuntosDeControl == null || !command.TotalPuntosDeControl.Any())
        {
            logger.LogError("No se puede iniciar la simulación para la maratón {IdCarrera}: Falta la lista de Checkpoints. Total Checkpoints: 0", command.IdCarrera);
            return;
        }

        logger.LogInformation("Iniciando simulación para el maratón {IdCarrera}.", command.IdCarrera);

        var geoReader = new GeoJsonReader();
        FeatureCollection featureCollection;
        try
        {
            featureCollection = geoReader.Read<FeatureCollection>(command.RutaGeoJson);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al leer o deserializar el GeoJSON para la maratón {IdCarrera}.", command.IdCarrera);
            return;
        }

        //extraer contorno del recorrido
        var routeFeature = featureCollection
        .FirstOrDefault(f => f.Geometry is LineString);

        if (routeFeature == null || routeFeature.Geometry is not LineString routeGeometry)
        {
            logger.LogError("No se encontró el contorno del recorrido requerido en el GeoJSON para la maratón {IdCarrera}.", command.IdCarrera);
            return;
        }
        var rutaIndexada = new LengthIndexedLine(routeGeometry);

        // 🔥 CAMBIO 1: BARAJAR CORREDORES ANTES DE SIMULAR
        // Creamos una copia del comando con la lista desordenada para que el método Simular use ese orden
        var listaBarajada = command.IdCorredores.OrderBy(_ => _random.Next()).ToList();
        var commandBarajado = command with { IdCorredores = listaBarajada };

        //simulacion
        var simulacionCompleta = SimularCarreraCompleta(commandBarajado, rutaIndexada);
        logger.LogInformation("Simulación para la maratón {IdCarrera} finalizada. Enviando eventos", command.IdCarrera);


        var startCoordinate = rutaIndexada.ExtractPoint(0); 
        var corredoresIniciales = new List<CorredorData>();

        foreach (var id in command.IdCorredores)
        {
            corredoresIniciales.Add(new CorredorData(
                command.IdCarrera,
                id,
                0, 
                0, 
                startCoordinate.Y, 
                startCoordinate.X,
                0,
                TimeSpan.Zero,
                TimeSpan.Zero // 🔥 CAMBIO 2: Tiempo Neto Inicial 0
            ));
        }

        var eventoInicio = new CarreraIniciada(
            command.IdCarrera,
            corredoresIniciales,
            command.TotalPuntosDeControl
        );

        await bus.Publish(eventoInicio);
        _ = Task.Run(async () =>
        {
            try
            {
                await EnviarEventosEnTiempoReal(command.IdCarrera, simulacionCompleta, command.IdCorredores.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al enviar eventos para la maratón {IdCarrera}", command.IdCarrera);
            }
        });
        await Task.CompletedTask;
    }

    private Dictionary<int, List<EventoCorredor>> SimularCarreraCompleta(
    IniciarCarreraCommand command,
    LengthIndexedLine rutaIndexada)
    {
        var eventosPorCorredor = new Dictionary<int, List<EventoCorredor>>();
        var tiemposSalida = CalcularTiemposDeSalida(command.IdCorredores);

        var checkpointsOrdenados = command.TotalPuntosDeControl!
            .OrderBy(kvp => kvp.Value)
            .ToList();

        var distanciaTotalCarreraKm = checkpointsOrdenados.Last().Value;
        var longitudTotalGeometria = rutaIndexada.EndIndex;

        if (distanciaTotalCarreraKm <= 0) return eventosPorCorredor;

        foreach (var idCorredor in command.IdCorredores)
        {
            var eventos = new List<EventoCorredor>();
            var tiemposPorTramo = new List<TiempoPorTramoDTO>();
            var tiempoSalida = tiemposSalida[idCorredor];
            
            // Variables de tiempo acumulado
            var tiempoOficialAcumulado = TimeSpan.Zero; // Desde disparo inicial (incluye espera en salida)
            var tiempoNetoAcumulado = TimeSpan.Zero;    // 🔥 NUEVO: Tiempo real corriendo

            var ritmoBaseMinPorKm = 4.0 + _random.NextDouble() * 4.0;
            double kmAnterior = 0.0;

            // Evento SALIDA (Km 0)
            // Es importante agregarlo para que el frontend sepa que ya salió
            var startPoint = rutaIndexada.ExtractPoint(0);
            eventos.Add(new EventoCorredor
            {
                IdCorredor = idCorredor,
                CheckpointId = 0,
                Km = 0,
                VelocidadKmh = 0,
                Latitud = startPoint.Y,
                Longitud = startPoint.X,
                TiempoSalida = tiempoSalida,
                TiempoOficial = tiempoSalida, // En el reloj oficial es la hora de salida
                TiempoReal = TimeSpan.Zero    // Pero lleva 0 minutos corriendo
            });

            for (int i = 0; i < checkpointsOrdenados.Count; i++)
            {
                var checkpoint = checkpointsOrdenados[i];
                var idCheckPoint = checkpoint.Key;
                var kmActual = checkpoint.Value;

                float distanciaTramo = (float)(kmActual - kmAnterior);

                if (distanciaTramo <= 0) continue; // Evitar checkpoints duplicados o en 0 si ya agregamos la salida

                // ritmo
                var variacion = 0.75 + _random.NextDouble() * 0.5;
                var ritmoTramo = ritmoBaseMinPorKm * variacion * (1.0 + (i * 0.02)); // Factor fatiga

                var tiempoTramo = TimeSpan.FromMinutes(distanciaTramo * ritmoTramo);
                
                // Acumuladores
                // Tiempo Oficial = Tiempo Salida + Tiempo Corriendo
                tiempoOficialAcumulado = tiempoSalida + tiempoNetoAcumulado + tiempoTramo; 
                // Tiempo Neto = Solo Tiempo Corriendo
                tiempoNetoAcumulado += tiempoTramo;

                float velocidadKmh = 0f;
                if (tiempoTramo.TotalHours > 0 && distanciaTramo > 0)
                {
                    velocidadKmh = (float)(distanciaTramo / tiempoTramo.TotalHours);
                }

                var desdePuntoId = i == 0 ? 0 : checkpointsOrdenados[i - 1].Key;
                tiemposPorTramo.Add(new TiempoPorTramoDTO(desdePuntoId, idCheckPoint, tiempoTramo));

                double distanciaEnMapa = (kmActual / distanciaTotalCarreraKm) * longitudTotalGeometria;
                distanciaEnMapa = Math.Clamp(distanciaEnMapa, 0, longitudTotalGeometria);
                Coordinate coordenada = rutaIndexada.ExtractPoint(distanciaEnMapa);

                double lat = coordenada.Y;
                double lon = coordenada.X;
                if (double.IsNaN(lat) || double.IsInfinity(lat)) { lat = 0; lon = 0; }

                eventos.Add(new EventoCorredor
                {
                    IdCorredor = idCorredor,
                    TiempoReal = tiempoNetoAcumulado,     // 🔥 TIEMPO NETO
                    TiempoOficial = tiempoOficialAcumulado, // TIEMPO RELOJ
                    TiempoSalida = tiempoOficialAcumulado,  // MOMENTO SIMULACIÓN (Coincide con oficial)
                    CheckpointId = idCheckPoint,
                    Km = kmActual,
                    VelocidadKmh = velocidadKmh,
                    TiemposPorTramo = new List<TiempoPorTramoDTO>(tiemposPorTramo),
                    Latitud = lat,
                    Longitud = lon,
                });

                kmAnterior = kmActual;
            }
            eventosPorCorredor[idCorredor] = eventos;
        }

        return eventosPorCorredor;
    }

    private Dictionary<int, TimeSpan> CalcularTiemposDeSalida(List<int> idCorredores)
    {
        var tiemposSalida = new Dictionary<int, TimeSpan>();
        var tiempoBaseOleada = TimeSpan.Zero;
        var numeroOleada = 1;

        for (int i = 0; i < idCorredores.Count; i++)
        {
            if (i > 0 && i % CORREDORES_POR_OLEADA == 0)
            {
                tiempoBaseOleada += TimeSpan.FromSeconds(INTERVALO_ENTRE_OLEADAS_SEGUNDOS);
                numeroOleada++;
            }

            var variacionDentroOleada = TimeSpan.FromSeconds(_random.NextDouble() * VARIACION_SALIDA_DENTRO_OLEADA_MAX_SEGUNDOS);
            tiemposSalida[idCorredores[i]] = tiempoBaseOleada + variacionDentroOleada;
        }

        logger.LogInformation("Total de {TotalCorredores} corredores distribuidos en {NumOleadas} oleadas",
            idCorredores.Count,
            numeroOleada);

        return tiemposSalida;
    }

    private async Task EnviarEventosEnTiempoReal(
        int idCarrera,
        Dictionary<int, List<EventoCorredor>> simulacion,
        int totalCorredores)
    {
        var todosLosEventos = simulacion
            .SelectMany(kvp => kvp.Value.Select(e => new { Evento = e, IdCorredor = kvp.Key }))
            .OrderBy(x => x.Evento.TiempoSalida) // Ordenar cronológicamente por tiempo de simulación
            .ToList();

        var tiempoInicio = DateTime.UtcNow;
        var factorAceleracion = 5;

        foreach (var item in todosLosEventos)
        {
            // Usamos TiempoSalida (que es el TiempoOficial absoluto) para el delay
            var tiempoSimuladoEnSegundos = item.Evento.TiempoSalida.TotalSeconds;
            var tiempoRealEnSegundos = tiempoSimuladoEnSegundos / factorAceleracion;
            var tiempoObjetivo = tiempoInicio.AddSeconds(tiempoRealEnSegundos);
            var esperaMs = (int)(tiempoObjetivo - DateTime.UtcNow).TotalMilliseconds;
            if (esperaMs > 0)
            {
                await Task.Delay(esperaMs);
            }

            // 🔥 CAMBIO 3: Pasar TiempoReal (Neto) en el último parámetro si el constructor lo permite
            // Si tu DTO compartido no tiene el parámetro extra, usa TiempoReal en lugar de TiempoOficial
            // O agrega la propiedad como te mostré antes.
            var evento = new CorredorData(
                idCarrera,
                item.IdCorredor,
                item.Evento.VelocidadKmh,
                item.Evento.CheckpointId,
                item.Evento.Latitud,
                item.Evento.Longitud,
                item.Evento.Km,
                item.Evento.TiempoOficial,
                item.Evento.TiempoReal // <--- AQUÍ VA EL NETO
            );

            await bus.Publish(evento);
        }
        var eventoFinalizacion = new CarreraFinalizadaEvent(
            idCarrera,
            DateTime.UtcNow,
            totalCorredores,
            simulacion.Count
        );

        await bus.Publish(eventoFinalizacion);

        logger.LogInformation(
            "🎉 CARRERA {IdCarrera} FINALIZADA",
            idCarrera
        );
    }

    private class EventoCorredor
    {
        public int IdCorredor { get; set; }
        public TimeSpan TiempoReal { get; set; }   // Neto (corriendo)
        public TimeSpan TiempoOficial { get; set; } // Reloj (global)
        public TimeSpan TiempoSalida { get; set; }  // Momento del evento
        public int CheckpointId { get; set; }
        public double Km { get; set; }
        public double VelocidadKmh { get; set; }
        public List<TiempoPorTramoDTO> TiemposPorTramo { get; set; } = new();
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }

    public record TiempoPorTramoDTO(int DesdePuntoDeControlId, int HastaPuntoDeControlId, TimeSpan Tiempo);
}