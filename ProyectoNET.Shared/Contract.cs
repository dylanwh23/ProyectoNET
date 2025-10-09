namespace ProyectoNET.Shared;

public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);

public record TiempoPorTramoDTO(
    int DesdePuntoDeControlId,
    int HastaPuntoDeControlId,
    TimeSpan Tiempo 
);

public record ProgresoCorredorActualizado(
    int IdCarrera,
    int IdCorredor,
    PuntosDeControlDTO UltimoCheckpointPasado,
    float VelocidadKmh,
    List<TiempoPorTramoDTO> TiemposPorTramo
);