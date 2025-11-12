using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
using ProyectoNET.Carreras.API.Services;
namespace ProyectoNET.Carreras.API.Hubs;

public class CarreraHub : Hub
{
    // -------------------------
    // Conexión / Desconexión
    // -------------------------
    ILogger<CarreraHub> _logger;
    private readonly ICarreraStateService _stateService;
  public CarreraHub(ILogger<CarreraHub> logger, ICarreraStateService stateService) 
    {
        _logger = logger;
        _stateService = stateService; 
    }
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation("✅ Cliente conectado: {ConnectionId}", Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        _logger.LogInformation(exception != null
            ? $"🔌 Cliente desconectado por error: {Context.ConnectionId} ({exception.Message})"
            : $"🔌 Cliente desconectado normalmente: {Context.ConnectionId}");
    }

    // -------------------------
    // Métodos para unirse a grupos
    // -------------------------
    /// <summary>
    /// Permite al cliente unirse a la carrera indicada.
    /// </summary>
  public async Task UnirseCarrera(int carreraId)
    {
        string grupo = ObtenerNombreGrupo(carreraId);
        await Groups.AddToGroupAsync(Context.ConnectionId, grupo);
        _logger.LogInformation("Cliente {ConnectionId} se unió al grupo {Grupo}", Context.ConnectionId, grupo);
        await Clients.Caller.SendAsync("Log", $"✅ Conectado al grupo de carrera {carreraId}");

        // --- INICIO DE LA SOLUCIÓN ---
        // 4. Obtener el estado actual de la caché
        var (corredores, puntos, estadoActual) = _stateService.GetEstadoActual(carreraId);

        // 5. Si la carrera ya existe en la caché (ya inició),
        //    envía el estado completo SOLO a este cliente.
        if (corredores.Any())
        {
            _logger.LogInformation("La carrera {Id} ya inició. Enviando estado actual al cliente {ConnId}", carreraId, Context.ConnectionId);

            // ¡Enviamos los datos de "CarreraIniciada" al cliente que acaba de hacer F5!
            // Tu Blazor ya sabe cómo manejar esto (OnCarreraIniciada)
            await Clients.Caller.SendAsync("CarreraIniciada", corredores, puntos);

            // ¡NUEVO! Enviamos el estado más reciente de todos los corredores
            // Tu Blazor necesitará un nuevo listener para esto
            await Clients.Caller.SendAsync("EstadoCompletoRecibido", estadoActual);
        }
        // --- FIN DE LA SOLUCIÓN ---
    }

    /// <summary>
    /// Permite al cliente salir de un grupo/carrera.
    /// </summary>
    public async Task SalirCarrera(int carreraId)
    {
        string grupo = ObtenerNombreGrupo(carreraId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
        _logger.LogInformation("Cliente {ConnectionId} salió del grupo {Grupo}", Context.ConnectionId, grupo);
        await Clients.Caller.SendAsync("Log", $"⚠️ Saliste del grupo de carrera {carreraId}");
    }

    // -------------------------
    // Envío de progreso
    // -------------------------
    /// <summary>
    /// Envía el progreso únicamente a los clientes conectados al grupo de la carrera.
    /// </summary>
    public async Task EnviarProgreso(int carreraId, CarreraData data)
    {
        string grupo = ObtenerNombreGrupo(carreraId);
        await Clients.Group(grupo).SendAsync("RecibirProgreso", data);
        _logger.LogInformation("Progreso enviado al grupo {Grupo}: Corredor {CorredorId} - Tramo {TramosCompletados}", grupo, data.CorredorId, data.TramosCompletados);
    }

    // -------------------------
    // Helpers
    // -------------------------
    private string ObtenerNombreGrupo(int carreraId) => $"carrera-{carreraId}";

    // DTO de ejemplo para enviar progreso


    public async Task EnviarCarreraIniciada(int carreraId,CarreraIniciadaEvent evento)
    {
        string grupo = ObtenerNombreGrupo(carreraId);
        await Clients.Group(grupo).SendAsync("CarreraIniciada", evento);
        _logger.LogInformation("Evento 'CarreraIniciada' enviado al grupo {Grupo}", grupo);
    }
   
}
