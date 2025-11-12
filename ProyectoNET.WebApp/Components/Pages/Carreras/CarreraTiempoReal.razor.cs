using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp; // O donde estén tus DTOs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; // ¡Importante!
using System.Threading.Tasks;

// Asegúrate de que este namespace coincida con la ubicación de tu archivo .razor
namespace ProyectoNET.WebApp.Components.Pages.Carreras
{
    public partial class CarreraTiempoReal : ComponentBase, IAsyncDisposable
    {
        [Parameter]
        public string? CarreraId { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; } = default!;

        [Inject]
        private ILogger<CarreraTiempoReal> Logger { get; set; } = default!;

        private HubConnection? _hubConnection;
        private Random _random = new Random();

        // --- ESTADO DE LA CARRERA ---
        private List<string> _logMessages = new List<string>();
        
        // --- ESTADO PARA LA UI ---
        private bool _carreraIniciada = false;
        private float TotalKmCarrera = 10.0f; // Valor por defecto
        private List<PuntosDeControlDTO> _puntosDeControl = new();
        
        // --- ESTADO CENTRALIZADO ---
        private Dictionary<int, CarreraData> _estadoCorredores = new(); 
        private Dictionary<int, CarreraData> _estadoVisualCorredores = new();
        private List<CarreraData> _listaOrdenadaCorredores = new();
        private List<CarreraData> _corredoresMostrados = new();
        private Dictionary<int, string> _coloresCorredores = new();
        
        // --- LÓGICA DE PAGINACIÓN Y BÚSQUEDA ---
        private int _paginaActual = 0;
        private int _tamañoPagina = 10;
        private int _totalPaginas = 0;
        private int? _corredorResaltadoId = null;
        private string _terminoBusqueda = string.Empty;
        private string TerminoBusqueda
        {
            get => _terminoBusqueda;
            set { _terminoBusqueda = value; ActualizarPaginaYFiltros(); }
        }
        
        // --- MOTOR DE ANIMACIÓN DEL CLIENTE ---
        private Timer? _animationTimer;
        private DateTime _lastTickTime;
        private const int TicksPorSegundo = 1; // 30 FPS

        protected override async Task OnInitializedAsync()
        {
            if (!int.TryParse(CarreraId, out var carreraIdNum)) { Logger.LogError($"CarreraId no es válido: '{CarreraId}'."); return; }
            var apiUrl = Configuration["services:carreras-api:https:0"] ?? Configuration["services:carreras-api:http:0"];
            if (string.IsNullOrEmpty(apiUrl)) { Logger.LogError("No se pudo encontrar la URL del servicio 'carreras-api'."); return; }
            var hubUrl = $"{apiUrl.TrimEnd('/')}/carrerahub";
            Logger.LogInformation($"Conectando al Hub en: {hubUrl}");
            _hubConnection = new HubConnectionBuilder().WithUrl(hubUrl).WithAutomaticReconnect().Build();
            RegisterHubListeners();
            try
            {
                await _hubConnection.StartAsync();
                Logger.LogInformation("Conectado al Hub.");
                await _hubConnection.InvokeAsync("UnirseCarrera", carreraIdNum);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al conectar o unirse al Hub.");
                _logMessages.Add($"Error de conexión: {ex.Message}");
            }
        }
        
        // --- SECCIÓN DE LISTENERS ---

        private void RegisterHubListeners()
        {
            if (_hubConnection == null) return;
            _hubConnection.On<string>("Log", OnLogReceived);
            _hubConnection.On<List<int>, List<PuntosDeControlDTO>>("CarreraIniciada", OnCarreraIniciada);
            _hubConnection.On<Dictionary<int, CarreraData>>("EstadoCompletoRecibido", OnEstadoCompletoRecibido);
        }

        // --- HANDLERS DE SIGNALR ---

        private void OnLogReceived(string message)
        {
            Logger.LogInformation($"Mensaje del Hub: {message}");
            _logMessages.Add(message);
            InvokeAsync(StateHasChanged);
        }
        
        private void OnCarreraIniciada(List<int> idCorredores, List<PuntosDeControlDTO> puntosDeControl)
        {
            Logger.LogInformation("Recibidos datos de setup: {Count} corredores y {Puntos} puntos.", idCorredores.Count, puntosDeControl.Count);

            _puntosDeControl = puntosDeControl;
            if (_puntosDeControl.Any())
                TotalKmCarrera = _puntosDeControl.Max(p => p.Km);

            if (!_estadoCorredores.Any())
            {
                foreach (var id in idCorredores)
                {
                    var data = new CarreraData 
                    { 
                        CorredorId = id, Checkpoint = "En la salida", KmRecorridos = 0, Velocidad = 0,
                        CarreraId = int.Parse(CarreraId ?? "0")
                    };
                    _estadoCorredores[id] = data; 
                    _estadoVisualCorredores[id] = new CarreraData(data); // Constructor de copia
                    
                    if (!_coloresCorredores.ContainsKey(id))
                        _coloresCorredores[id] = GetRandomColor();
                }
            }
            
            _carreraIniciada = true;
            _logMessages.Add("🟢 ¡Carrera iniciada! Preparando estado...");
            
            if (_animationTimer == null)
            {
                _lastTickTime = DateTime.UtcNow;
                _animationTimer = new Timer(OnAnimationTick, null, 1000/TicksPorSegundo, 1000/TicksPorSegundo);
            }
            
            ActualizarPaginaYFiltros();
            InvokeAsync(StateHasChanged);
        }

        private void OnEstadoCompletoRecibido(Dictionary<int, CarreraData> estado)
        {
            _estadoCorredores = estado;
            
            foreach (var id in _estadoCorredores.Keys)
            {
                if (!_coloresCorredores.ContainsKey(id))
                    _coloresCorredores[id] = GetRandomColor();
            }
        }
        
        // --- MOTOR DE ANIMACIÓN ---
        
        private void OnAnimationTick(object? state)
        {
            if (!_carreraIniciada || !_estadoCorredores.Any()) return;

            var now = DateTime.UtcNow;
            var deltaSeconds = (now - _lastTickTime).TotalSeconds;
            _lastTickTime = now;
            bool needsRender = false;

            foreach (var id in _estadoCorredores.Keys)
            {
                var truth = _estadoCorredores[id];
                
                if (!_estadoVisualCorredores.TryGetValue(id, out var visual))
                {
                    visual = new CarreraData(truth);
                    _estadoVisualCorredores[id] = visual;
                }
                
                if (truth.KmRecorridos > visual.KmRecorridos && visual.Velocidad == 0)
                {
                    visual.KmRecorridos = truth.KmRecorridos;
                    visual.Velocidad = truth.Velocidad; 
                    visual.Checkpoint = truth.Checkpoint;
                    needsRender = true;
                }
                else if (visual.Velocidad > 0)
                {
                    var nextCheckpointKm = GetNextCheckpointKm(visual.KmRecorridos);
                    var kmToAdd = (float)((visual.Velocidad / 3600.0) * deltaSeconds);
                    var newKm = visual.KmRecorridos + kmToAdd;

                    if (newKm >= truth.KmRecorridos && truth.KmRecorridos > visual.KmRecorridos)
                    {
                        visual.KmRecorridos = truth.KmRecorridos;
                        visual.Velocidad = truth.Velocidad; 
                        visual.Checkpoint = truth.Checkpoint;
                    }
                    else if (newKm < nextCheckpointKm)
                    {
                        visual.KmRecorridos = newKm;
                    }
                    else
                    {
                        visual.KmRecorridos = nextCheckpointKm;
                        visual.Velocidad = 0; 
                    }
                    needsRender = true;
                }
            }

            if (needsRender)
            {
                ActualizarPaginaYFiltros();
                InvokeAsync(StateHasChanged);
            }
        }
        
        private void ActualizarPaginaYFiltros()
        {
            if (!_estadoVisualCorredores.Any()) return;

            IEnumerable<CarreraData> corredoresFiltrados;

            if (!string.IsNullOrWhiteSpace(TerminoBusqueda) && int.TryParse(TerminoBusqueda, out int idBuscado))
            {
                _totalPaginas = 1;
                _paginaActual = 0;
                
                if (_estadoVisualCorredores.TryGetValue(idBuscado, out var corredorBuscado))
                {
                    corredoresFiltrados = new List<CarreraData> { corredorBuscado };
                }
                else
                {
                    corredoresFiltrados = Enumerable.Empty<CarreraData>();
                }
            }
            else
            {
                _listaOrdenadaCorredores = _estadoVisualCorredores.Values
                    .OrderByDescending(x => x.KmRecorridos)
                    .ThenBy(x => x.CorredorId)
                    .ToList();
                
                _totalPaginas = (int)Math.Ceiling(_listaOrdenadaCorredores.Count / (double)_tamañoPagina);
                
                corredoresFiltrados = _listaOrdenadaCorredores
                    .Skip(_paginaActual * _tamañoPagina)
                    .Take(_tamañoPagina);
            }
            
            _corredoresMostrados = corredoresFiltrados.ToList();
        }
        
        // --- Métodos de Paginación y Resaltado ---
        private void PaginaSiguiente()
        {
            if (_paginaActual < _totalPaginas - 1) { _paginaActual++; ActualizarPaginaYFiltros(); }
        }
        private void PaginaAnterior()
        {
            if (_paginaActual > 0) { _paginaActual--; ActualizarPaginaYFiltros(); }
        }
        private void ResaltarCorredor(int id) => _corredorResaltadoId = id;
        private void QuitarResaltado() => _corredorResaltadoId = null;

        // --- Helpers ---
        private string GetRandomColor()
        {
            int hue = _random.Next(0, 360); int saturation = _random.Next(70, 101); int lightness = _random.Next(40, 61);
            return $"hsl({hue}, {saturation}%, {lightness}%)";
        }

        private string GetPosicionPorcentaje(float km)
        {
            var porcentaje = (km / TotalKmCarrera) * 100;
            return $"{Math.Min(porcentaje, 100):F2}";
        }

        private float GetNextCheckpointKm(float currentKm)
        {
            var nextCheckpoint = _puntosDeControl.FirstOrDefault(p => p.Km > currentKm);
            return nextCheckpoint?.Km ?? TotalKmCarrera;
        }

        // --- ¡EL MÉTODO QUE FALTABA! ---
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                Logger.LogInformation("Limpiando conexión al Hub...");
                if (int.TryParse(CarreraId, out var carreraIdNum))
                {
                    try { await _hubConnection.InvokeAsync("SalirCarrera", carreraIdNum); }
                    catch (Exception ex) { Logger.LogWarning($"Error (ignorable) al salir del grupo: {ex.Message}"); }
                }
                await _hubConnection.DisposeAsync();
            }
            
            // ¡Importante! Detener el timer
            _animationTimer?.Dispose();
        }
    }
}