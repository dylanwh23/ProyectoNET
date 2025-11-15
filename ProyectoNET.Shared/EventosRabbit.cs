namespace ProyectoNET.Shared.EventosRabbit;
//
public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);
//
public record CorredorData(int IdCarrera, int IdCorredor, double Velocidad, int UltimoCheckpoint);
//
public record CarreraFinalizadaEvent(int IdCarrera, DateTime? FechaFin, int TotalCorredores, int CorredoresFinalizados);
//
public record CarreraIniciada(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);
 
