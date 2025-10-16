using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace ProyectoNET.Carreras.API.Hubs;

public class CarreraHub : Hub
{
    // Si tuvieras un constructor, se vería así:
    // private readonly ILogger<CarreraHub> _logger;
    // public CarreraHub(ILogger<CarreraHub> logger) {
    //     _logger = logger;
    // }

    public override async Task OnConnectedAsync()
    {
        try
        {
            // Este código se ejecuta cuando un nuevo cliente (tu WebApp) se conecta.
            await base.OnConnectedAsync();
            Console.WriteLine($"✅ Cliente conectado exitosamente: {Context.ConnectionId}");
        }
        catch (Exception ex)
        {
            // ¡ESTO ES LO IMPORTANTE!
            // Si ocurre un error durante la conexión, lo veremos aquí en la consola de la API.
            Console.WriteLine($"❌❌❌ ERROR GRAVE en OnConnectedAsync: {ex.Message}");
            Console.WriteLine(ex.ToString()); // Imprime todos los detalles del error para encontrar la causa.

            // Forzamos el cierre de la conexión para que el cliente no se quede "colgado".
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Este código se ejecuta cuando un cliente se desconecta.
        await base.OnDisconnectedAsync(exception);

        if (exception != null)
        {
            // Si la desconexión fue por un error, lo veremos aquí.
            Console.WriteLine($"🔌 Cliente desconectado POR ERROR: {Context.ConnectionId}");
            Console.WriteLine($"   Motivo del error: {exception.Message}");
        }
        else
        {
            Console.WriteLine($"🔌 Cliente desconectado normalmente: {Context.ConnectionId}");
        }
    }
}
