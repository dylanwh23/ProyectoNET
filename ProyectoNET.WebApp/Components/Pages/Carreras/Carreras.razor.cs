using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client; // <--- Paquete SignalR.Client
using ProyectoNET.Shared;
namespace ProyectoNET.WebApp.Components.Pages.Carreras
{
    public partial class Carreras : ComponentBase
    {
        private List<Carrera> _carreras = new List<Carrera>();

        protected override async Task OnInitializedAsync()
        {
            
        }

    }
       
}