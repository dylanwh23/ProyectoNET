using Microsoft.AspNetCore.SignalR;

namespace ProyectoNET.Carreras.API.Hubs;

public class CarreraHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"🟢 Cliente conectado: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"🔴 Cliente desconectado: {Context.ConnectionId}");
    }
}
