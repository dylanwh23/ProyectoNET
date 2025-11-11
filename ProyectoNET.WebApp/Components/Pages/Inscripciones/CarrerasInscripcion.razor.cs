using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;    // <--- Añadir
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
namespace ProyectoNET.WebApp.Components.Pages.Inscripciones
{
    public partial class CarrerasInscripcion: ComponentBase
    {
         private List<CarreraInscripcionCard> _carreras = new List<CarreraInscripcionCard>();

        // Inyectamos la "fábrica" para crear el cliente que configuramos
        [Inject]
        private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        // (Opcional pero recomendado) Inyectamos un logger para ver errores
        [Inject]
        private ILogger<CarrerasInscripcion> Logger { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                // 1. Crea el cliente HTTP usando el nombre "api" que registramos
                var httpClient = HttpClientFactory.CreateClient("api");

                // 2. Llama a tu endpoint
                // ❗️ Ajusta "api/carreras" a la ruta real de tu controlador de API
                var result = await httpClient.GetFromJsonAsync<List<CarreraInscripcionCard>>("api/carreras/inscripcion");

                // 3. Asigna el resultado a tu lista
                if (result != null)
                {
                    _carreras = result;
                }
                
            }
            catch (Exception ex)
            {
                // Si algo falla (la API está caída, el JSON no coincide, etc.)
                // lo veremos en la consola.
                Logger.LogError(ex, "Error al cargar la lista de carreras desde la API.");
                // Opcional: podrías poner un mensaje de error para el usuario
            }
        }
    }
}