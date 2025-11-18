namespace ProyectoNET.Shared.EventosRabbit;
//
public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);
//
public record CorredorData
{
    public int IdCarrera{ get; init; }
    public int IdCorredor{ get; init; }
    public double Velocidad{ get; init; }
    public int UltimoCheckpoint{ get; init; }
    public CorredorData() { }

    public CorredorData(int idCarrera, int idCorredor, double velocidad, int checkpoint)
    {
        IdCarrera = idCarrera;
        IdCorredor = idCorredor;
        Velocidad = velocidad;
        UltimoCheckpoint = checkpoint;
    }

}
//
public record CarreraFinalizadaEvent(int IdCarrera, DateTime? FechaFin, int TotalCorredores, int CorredoresFinalizados);
//
public record CarreraIniciada(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);
 
