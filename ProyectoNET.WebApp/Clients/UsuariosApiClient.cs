namespace ProyectoNET.WebApp.Clients;
public class UsuariosApiClient(HttpClient httpClient)
{
    public async Task<string> GetUsuariosTestAsync()
    {
        try
        {
            return await httpClient.GetStringAsync("/usuarios-test");
        }
        catch (Exception ex)
        {
            return $"Error al conectar con Usuarios.API: {ex.Message}";
        }
    }
}