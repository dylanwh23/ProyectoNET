using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client; // <--- Paquete SignalR.Client
using ProyectoNET.Shared;
namespace ProyectoNET.WebApp.Components.Pages.Carreras
{
    public partial class CarreraTiempoReal : ComponentBase, IAsyncDisposable
    {
        [Parameter]
        public string? CarreraId { get; set; }
        [Inject]
        private IConfiguration Configuration { get; set; } = default!;
        private HubConnection? _hubConnection;
        [Inject]
        private ILogger<CarreraTiempoReal> Logger { get; set; } = default!;
        private Random _random = new Random();

        private HttpClient _httpClient = new HttpClient();

        //prueba 
        private List<string> _logMessages = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            // Validar CarreraId ANTES de hacer nada
            if (!int.TryParse(CarreraId, out var carreraIdNum))
            {
                Logger.LogError($"CarreraId no es válido: '{CarreraId}'. No se puede conectar al Hub.");
                _logMessages.Add($"Error: El ID de carrera '{CarreraId}' no es válido.");
                return;
            }

            var apiUrl = Configuration["services:carreras-api:https:0"] ?? Configuration["services:carreras-api:http:0"];
            if (string.IsNullOrEmpty(apiUrl))
            {
                Logger.LogError("No se pudo encontrar la URL del servicio 'carreras-api' en IConfiguration.");
                return;
            }

            var hubUrl = $"{apiUrl.TrimEnd('/')}/carrerahub";
            Logger.LogInformation($"Conectando al Hub en: {hubUrl}");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl) // <--- Se quitó la línea 'options' que daba error
                .WithAutomaticReconnect()
                .Build();

            // --- Registrar listeners ANTES de conectar ---

            RegisterHubListeners();

            // --- Conectar y Unirse al Grupo ---
            try
            {
                await _hubConnection.StartAsync();
                Logger.LogInformation("Conectado al Hub de carreras en tiempo real.");

                // *** LA PARTE QUE FALTABA ***
                await _hubConnection.InvokeAsync("UnirseCarrera", carreraIdNum);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al conectar o unirse al Hub.");
                _logMessages.Add($"Error de conexión: {ex.Message}");
            }
        }

        private string GetRandomColor()
        {
            int hue = _random.Next(0, 360);
            int saturation = _random.Next(70, 101);
            int lightness = _random.Next(40, 61);
            return $"hsl({hue}, {saturation}%, {lightness}%)";
        }


        public async ValueTask DisposeAsync()
        {
            if (_hubConnection is not null)
            {
                Logger.LogInformation("Limpiando conexión al Hub...");
                if (int.TryParse(CarreraId, out var carreraIdNum))
                {
                    try
                    {
                        // Avisamos al hub que nos vamos
                        await _hubConnection.InvokeAsync("SalirCarrera", carreraIdNum);
                    }
                    catch (Exception ex) { Logger.LogWarning($"Error (ignorable) al salir del grupo: {ex.Message}"); }
                }

                await _hubConnection.DisposeAsync();
            }
        }


        private void RegisterHubListeners()
        {
            if (_hubConnection == null) return;
            _hubConnection.On<string>("Log", OnLogReceived);
            _hubConnection.On<CarreraData>("RecibirProgreso", OnProgresoReceived);
        }

        private void OnLogReceived(string message)
        {
            Logger.LogInformation($"Mensaje del Hub: {message}");
            _logMessages.Add(message);
            InvokeAsync(StateHasChanged);
        }


        private void OnProgresoReceived(CarreraData data)
        {
            Logger.LogInformation($"Progreso recibido: Corredor {data.CorredorId} -> {data.Checkpoint}");
            InvokeAsync(StateHasChanged);
        }

        

    }
}