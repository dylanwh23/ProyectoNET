using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ProyectoNET.Carreras.API.Hubs;

public class CarreraHub : Hub
{
    // -------------------------
    // Conexión / Desconexión
    // -------------------------
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"✅ Cliente conectado: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine(exception != null
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
        Console.WriteLine($"Cliente {Context.ConnectionId} se unió al grupo {grupo}");

        // Opcional: enviar log al cliente que se acaba de unir
        await Clients.Caller.SendAsync("Log", $"✅ Conectado al grupo de carrera {carreraId}");
    }

    /// <summary>
    /// Permite al cliente salir de un grupo/carrera.
    /// </summary>
    public async Task SalirCarrera(int carreraId)
    {
        string grupo = ObtenerNombreGrupo(carreraId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, grupo);
        Console.WriteLine($"Cliente {Context.ConnectionId} salió del grupo {grupo}");
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
        Console.WriteLine($"Progreso enviado al grupo {grupo}: Corredor {data.CorredorId} - Tramo {data.TramosCompletados}");
    }

    // -------------------------
    // Helpers
    // -------------------------
    private string ObtenerNombreGrupo(int carreraId) => $"carrera-{carreraId}";

    // DTO de ejemplo para enviar progreso
    public class CarreraData
    {
        public int CarreraId { get; set; }
        public int CorredorId { get; set; }
        public string Checkpoint { get; set; } = string.Empty;
        public double Velocidad { get; set; }
        public int TramosCompletados { get; set; }
    }
}
