namespace ProyectoNET.Shared.WebApp
{
    public record CarreraIniciadaEvent(
        int IdCarrera,
        List<int> IdCorredores,
        List<PuntosDeControlDTO> TotalPuntosDeControl
    );
}