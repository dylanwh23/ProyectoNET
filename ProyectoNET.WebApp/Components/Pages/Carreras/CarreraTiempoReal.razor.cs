using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using Microsoft.JSInterop;
using ProyectoNET.Shared.EventosRabbit;

namespace ProyectoNET.WebApp.Pages 
{
    public partial class CarreraTiempoReal : ComponentBase, IAsyncDisposable
    {
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        [Inject] IJSRuntime JSRuntime { get; set; } = default!; 
        [Inject] IHttpClientFactory HttpClientFactory { get; set; } = default!;

        [Parameter] public string CarreraId { get; set; } = string.Empty;

        protected HubConnection? _hubConnection;
        protected readonly ConcurrentDictionary<int, CorredorData> _runnersState = new(); 
        protected int _leaderId = 0;
        protected int? _searchedId = null;
        protected string _searchInputValue = string.Empty;
        protected List<CorredorData> _filteredRunners = new(); 

        protected bool _isDataReady = false;
        protected string _rutaGeoJson = string.Empty;
        private bool _mapInitialized = false;
        private CancellationTokenSource _cts = new(); 
        
        // 🔍 SISTEMA DE DEBOUNCE PARA BÚSQUEDA
        private System.Threading.Timer? _searchDebounceTimer;
        private const int DEBOUNCE_DELAY_MS = 300;

        protected override async Task OnInitializedAsync()
        {
            _ = StartRoutePollingLoop(_cts.Token);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/carrerashub"))
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("ResetLocalState", () => {
                _runnersState.Clear();
                _filteredRunners.Clear();
                InvokeAsync(StateHasChanged);
            });

            // Carga masiva inicial o lotes
            _hubConnection.On<List<CorredorData>>("ReceiveRaceUpdateBatch", (batch) => {
                foreach (var data in batch) 
                {
                    if (data.IdCorredor != 0) 
                        _runnersState[data.IdCorredor] = data;
                }
                UpdateUI();
            });

            // ACTUALIZACIÓN INDIVIDUAL
            _hubConnection.On<CorredorData>("ReceiveRaceUpdate", (data) => {
                if (data.IdCorredor != 0) {
                    _runnersState[data.IdCorredor] = data;
                    UpdateUI();
                }
            });

            try {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("JoinRaceGroup", CarreraId);
            } catch (Exception ex) { 
                Console.WriteLine($"Error SignalR: {ex.Message}"); 
            }
        }

        // 🔍 MÉTODO OPTIMIZADO PARA BÚSQUEDA CON DEBOUNCE
        protected void OnSearchKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
        {
            // Cancelar el timer anterior si existe
            _searchDebounceTimer?.Dispose();
            
            // Crear un nuevo timer que ejecute la búsqueda después del delay
            _searchDebounceTimer = new System.Threading.Timer(_ => {
                InvokeAsync(() => {
                    ProcessSearch();
                    StateHasChanged();
                });
            }, null, DEBOUNCE_DELAY_MS, Timeout.Infinite);
        }

        // 🔍 PROCESAR BÚSQUEDA - OPTIMIZADO
        private void ProcessSearch()
        {
            if (string.IsNullOrWhiteSpace(_searchInputValue))
            {
                _searchedId = null;
                UpdateUI();
                return;
            }
            
            if (int.TryParse(_searchInputValue.Trim(), out int searchId))
            {
                // 🚀 OPTIMIZACIÓN: Verificar directamente en el diccionario sin iterar
                // TryGetValue es O(1) en lugar de buscar en toda la colección
                if (_runnersState.ContainsKey(searchId))
                {
                    _searchedId = searchId;
                }
                else
                {
                    _searchedId = null;
                }
            }
            else
            {
                _searchedId = null;
            }
            
            UpdateUI();
        }

        // 🔍 LIMPIAR BÚSQUEDA
        protected void ClearSearch()
        {
            _searchInputValue = string.Empty;
            _searchedId = null;
            UpdateUI();
        }

        private async Task StartRoutePollingLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var json = await FetchRaceRouteGeoJson(CarreraId);
                    if (!string.IsNullOrWhiteSpace(json) && json.Length > 50)
                    {
                        _rutaGeoJson = json;
                        _isDataReady = true;
                        await InvokeAsync(StateHasChanged); 
                        if (_mapInitialized) {
                            await InvokeAsync(async () => await JSRuntime.InvokeVoidAsync("MapManager.updateRoute", _rutaGeoJson));
                        }
                        return; 
                    }
                }
                catch { }
                await Task.Delay(1000, token);
            }
        }
        
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_isDataReady && !_mapInitialized)
            {
                try {
                    _mapInitialized = true;
                    await JSRuntime.InvokeVoidAsync("MapManager.initMap", "map-container");
                    await JSRuntime.InvokeVoidAsync("MapManager.updateRoute", _rutaGeoJson);
                    UpdateUI(); 
                } catch { _mapInitialized = false; }
            }
        }

        protected void UpdateUI()
        {
            // 🚀 OPTIMIZACIÓN: Calcular solo una vez los valores necesarios
            var allRunners = _runnersState.Values;
            var activeRunners = allRunners.Where(r => r.TiempoNeto.TotalSeconds > 0).ToList();
            
            // 1. TOP 3 por RITMO - Calcular solo para corredores activos
            List<CorredorData> top3Ritmo;
            if (activeRunners.Count > 0)
            {
                top3Ritmo = activeRunners
                    .OrderByDescending(r => r.Km / r.TiempoNeto.TotalHours)
                    .Take(3)
                    .ToList();
                
                _leaderId = top3Ritmo[0].IdCorredor;
            }
            else
            {
                top3Ritmo = new List<CorredorData>();
                _leaderId = 0;
            }

            // 2. TOP 10 por DISTANCIA RECORRIDA - Más eficiente
            var top10Distancia = allRunners
                .OrderByDescending(r => r.Km)
                .Take(10)
                .ToList();

            // 3. Combinar runners para el mapa (sin duplicados)
            var runnersForMap = new HashSet<int>(top3Ritmo.Select(r => r.IdCorredor));
            var filteredList = new List<CorredorData>(top3Ritmo);
            
            foreach (var runner in top10Distancia)
            {
                if (runnersForMap.Add(runner.IdCorredor))
                {
                    filteredList.Add(runner);
                }
            }
            
            // 4. Agregar el corredor buscado si existe y no está ya en la lista
            if (_searchedId.HasValue && _runnersState.TryGetValue(_searchedId.Value, out var searchedRunner))
            {
                if (runnersForMap.Add(_searchedId.Value))
                {
                    filteredList.Add(searchedRunner);
                }
            }
            
            _filteredRunners = filteredList;

            InvokeAsync(async () => {
                StateHasChanged(); 
                if (_mapInitialized) await RenderMapBatch(_filteredRunners); 
            });
        }
        
        private async Task RenderMapBatch(IEnumerable<CorredorData> runnersToRender)
        {
            var batchData = runnersToRender.Select(r => new {
                id = r.IdCorredor,
                lat = r.Latitud,
                lng = r.Longitud,
                km = r.Km,
                vel = r.Velocidad,
                tiempoNeto = r.TiempoNeto.ToString(@"hh\:mm\:ss"),
                type = (r.IdCorredor == _leaderId) ? "leader" : (r.IdCorredor == _searchedId ? "searched" : "normal"),
                info = $"<div class='text-center'><b>#{r.IdCorredor}</b><br>⚡ {r.Velocidad:F1} km/h<br>🏃 {r.Km:F2}km<br>⏱ {r.TiempoNeto.ToString(@"hh\:mm\:ss")}</div>"
            });
            try {
                await JSRuntime.InvokeVoidAsync("MapManager.updateRunnersBatch", batchData);
            } catch { }
        }

        private async Task<string> FetchRaceRouteGeoJson(string carreraId)
        {
            var httpClient = HttpClientFactory.CreateClient("api"); 
            try {
                var response = await httpClient.GetAsync($"api/carrera/{carreraId}/route");
                if (response.IsSuccessStatusCode) return await response.Content.ReadAsStringAsync();
                return "";
            } catch { return ""; }
        }

        public async ValueTask DisposeAsync()
        {
            _searchDebounceTimer?.Dispose();
            _cts.Cancel();
            _cts.Dispose();
            if (_hubConnection is not null) await _hubConnection.DisposeAsync();
        }
    }
}