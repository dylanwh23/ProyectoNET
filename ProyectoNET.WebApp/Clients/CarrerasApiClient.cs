namespace ProyectoNET.WebApp.Clients;

public class CarrerasApiClient(HttpClient httpClient)
{
    public async Task<string> GetCarrerasTestAsync()
    {
        try
        {
            return await httpClient.GetStringAsync("/carreras-test");
        }
        catch (Exception ex)
        {
            return $"Error al conectar con Carreras.API: {ex.Message}";
        }
    }
}