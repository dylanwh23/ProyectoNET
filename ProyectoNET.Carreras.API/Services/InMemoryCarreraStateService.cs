using System.Collections.Concurrent;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
namespace ProyectoNET.Carreras.API.Services;


// Implementación Singleton del estado
public class InMemoryCarreraStateService : ICarreraStateService
{
    // Diccionario principal: Key es el IdCarrera
    private readonly ConcurrentDictionary<int, CarreraState> _carreras = new();

    // Clase interna para guardar el estado de UNA carrera
    private class CarreraState
    {
        public List<int> IdCorredores { get; set; } = new();
        public List<PuntosDeControlDTO> PuntosDeControl { get; set; } = new();
        // El estado más reciente de CADA corredor
        public ConcurrentDictionary<int, CarreraData> EstadoCorredores { get; set; } = new();
    }

    public void InicializarCarrera(CarreraIniciadaEvent evento)
    {
        // 1. Crear el estado inicial
        var estadoCorredores = new ConcurrentDictionary<int, CarreraData>();
        foreach (var id in evento.IdCorredores)
        {
            estadoCorredores[id] = new CarreraData 
            {
                CorredorId = id,
                Checkpoint = "En la salida",
                Velocidad = 0,
                TramosCompletados = 0,
                CarreraId = evento.IdCarrera
            };
        }

        // 2. Crear la nueva carrera en el diccionario principal
        _carreras[evento.IdCarrera] = new CarreraState
        {
            IdCorredores = evento.IdCorredores,
            PuntosDeControl = evento.TotalPuntosDeControl,
            EstadoCorredores = estadoCorredores
        };
    }

    public void ActualizarProgreso(ProgresoCorredorActualizado evento)
    {
        if (!_carreras.TryGetValue(evento.IdCarrera, out var carreraState))
            return;

        var dataParaCliente = new CarreraData
        {
            CarreraId = evento.IdCarrera,
            CorredorId = evento.IdCorredor,
            Checkpoint = $"{evento.UltimoCheckpointPasado.Km}km ({evento.UltimoCheckpointPasado.IdPuntoDeControl})",
            Velocidad = evento.VelocidadKmh,
            TramosCompletados = evento.TiemposPorTramo.Count,
            KmRecorridos = evento.KmRecorridos
        };

        carreraState.EstadoCorredores[evento.IdCorredor] = dataParaCliente;
    }

    public (List<int> Corredores, List<PuntosDeControlDTO> Puntos, Dictionary<int, CarreraData> EstadoActual) GetEstadoActual(int carreraId)
    {
        if (_carreras.TryGetValue(carreraId, out var estado))
        {
            // Devuelve una copia de los datos
            return (estado.IdCorredores, estado.PuntosDeControl, new Dictionary<int, CarreraData>(estado.EstadoCorredores));
        }
        // Si no se encuentra, devuelve vacío
        return (new List<int>(), new List<PuntosDeControlDTO>(), new Dictionary<int, CarreraData>());
    }

    // Métodos para la Parte 2
    public List<int> GetCarrerasActivas() => _carreras.Keys.ToList();
    public Dictionary<int, CarreraData> GetEstadoCorredores(int carreraId)
    {
         return _carreras.TryGetValue(carreraId, out var e) 
            ? new Dictionary<int, CarreraData>(e.EstadoCorredores) 
            : new Dictionary<int, CarreraData>();
    }
}