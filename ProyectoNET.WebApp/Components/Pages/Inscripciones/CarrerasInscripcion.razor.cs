using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp;
using ProyectoNET.WebApp.Models;
using System.Text.Json;
using System.Text;
using Microsoft.JSInterop;

namespace ProyectoNET.WebApp.Components.Pages.Inscripciones
{
    public partial class CarrerasInscripcion : ComponentBase
    {
        private List<CarreraInscripcionCard> _carreras = new List<CarreraInscripcionCard>();

        // Variables para el modal de inscripción
        private InscripcionCarreraViewModel inscripcionModel = new();
        private int selectedCarreraId; // Variable de respaldo
        private string mensajeExito = "";
        private string mensajeError = "";
        private string passwordGenerada = "";
        private bool isProcessing = false;
        private string debugMessage = "";

        [Inject]
        private IHttpClientFactory HttpClientFactory { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private ILogger<CarrerasInscripcion> Logger { get; set; } = default!;

        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient("api");
                var result = await httpClient.GetFromJsonAsync<List<CarreraInscripcionCard>>("api/carreras/inscripcion");

                if (result != null)
                {
                    _carreras = result;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al cargar la lista de carreras desde la API.");
            }
        }

        private void CerrarModal()
        {
            inscripcionModel = new();
            mensajeExito = "";
            mensajeError = "";
            passwordGenerada = "";
            isProcessing = false;
            StateHasChanged();
        }

        // --- CORRECCIÓN PRINCIPAL AQUÍ ---
        private void ReiniciarFormulario(int carreraId)
        {
            selectedCarreraId = carreraId;

            // Al crear el nuevo modelo, le asignamos inmediatamente el ID
            inscripcionModel = new InscripcionCarreraViewModel
            {
                CarreraId = carreraId
            };

            Logger.LogInformation($"DEBUG: ReiniciarFormulario ejecutado. ID seleccionado: {carreraId}. Modelo actualizado: {inscripcionModel.CarreraId}");

            mensajeExito = "";
            mensajeError = "";
            passwordGenerada = "";
            isProcessing = false;

            StateHasChanged();
        }
        // -------------------------------

        private async Task ProcesarInscripcion(Microsoft.AspNetCore.Components.Forms.EditContext editContext)
        {
            if (editContext.Model is not InscripcionCarreraViewModel currentInscripcionModel)
            {
                Logger.LogError("El modelo de EditContext no es InscripcionCarreraViewModel.");
                mensajeError = "Error interno del formulario. Intenta nuevamente.";
                isProcessing = false;
                StateHasChanged();
                return;
            }

            // Asegurarse de que el ID esté presente, ya sea del modelo o de la variable de respaldo
            if (currentInscripcionModel.CarreraId == 0 && selectedCarreraId != 0)
            {
                currentInscripcionModel.CarreraId = selectedCarreraId;
            }

            Logger.LogInformation("Inicio de ProcesarInscripcion para carreraId: {CarreraId}", currentInscripcionModel.CarreraId);

            var carreraSeleccionada = _carreras.FirstOrDefault(c => c.Id == currentInscripcionModel.CarreraId);

            if (carreraSeleccionada == null)
            {
                mensajeError = $"No se ha encontrado la carrera con ID {currentInscripcionModel.CarreraId}.";
                Logger.LogWarning("Carrera no encontrada en la lista local.");
                isProcessing = false;
                StateHasChanged();
                return;
            }

            isProcessing = true;
            mensajeError = "";
            mensajeExito = "";

            try
            {
                var httpClient = HttpClientFactory.CreateClient("usuariosApi");

                // Crear el request asegurándonos de enviar el ID correcto
                var inscripcionRequest = new
                {
                    Nombre = currentInscripcionModel.Nombre,
                    Apellido = currentInscripcionModel.Apellido,
                    Email = currentInscripcionModel.Email,
                    CarreraId = currentInscripcionModel.CarreraId, // Usar el ID del modelo
                    FechaNacimiento = currentInscripcionModel.FechaNacimiento,
                    LugarDeEntregaId = currentInscripcionModel.LugarDeEntregaId
                };

                var json = JsonSerializer.Serialize(inscripcionRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("api/usuarios/inscripcion", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<InscripcionResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        mensajeExito = $"¡Felicidades, {currentInscripcionModel.Nombre}! Te has inscrito exitosamente a '{carreraSeleccionada.Nombre}'.";
                        passwordGenerada = result.GeneratedPassword ?? "";

                        Logger.LogInformation("Inscripción exitosa para ID Carrera: {Id}", currentInscripcionModel.CarreraId);

                        await ActualizarListaCarreras();
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    mensajeError = $"Error al procesar la inscripción: {response.ReasonPhrase}.";
                    Logger.LogError("Error API: {Content}", errorContent);
                }
            }
            catch (Exception ex)
            {
                mensajeError = "Error inesperado de conexión.";
                Logger.LogError(ex, "Excepción en ProcesarInscripcion");
            }
            finally
            {
                isProcessing = false;
                StateHasChanged();
            }
        }

        private async Task ActualizarListaCarreras()
        {
            try
            {
                var httpClient = HttpClientFactory.CreateClient("api");
                var result = await httpClient.GetFromJsonAsync<List<CarreraInscripcionCard>>("api/carreras/inscripcion");

                if (result != null)
                {
                    _carreras = result;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al actualizar la lista de carreras");
            }
        }

        private class InscripcionResponse
        {
            public int UserId { get; set; }
            public string? GeneratedPassword { get; set; }
            public string Message { get; set; } = "";
            public string Email { get; set; } = "";
        }
    }
}