using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
namespace ProyectoNET.Carreras.API.Hubs;

public class CarreraHub : Hub
{
    ILogger<CarreraHub> _logger;
  public CarreraHub(ILogger<CarreraHub> logger  ) 
    {
        _logger = logger;
    }
    
   
}
