namespace ProyectoNET.Shared.EventosRabbit;

// Puntos de control
public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);

// Comando para iniciar carrera
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);

// Datos del corredor durante la carrera
public record CorredorData
{
    public int IdCarrera { get; init; }
    public int IdCorredor { get; init; }
    public double Velocidad { get; init; }
    public int UltimoCheckpoint { get; init; }
    
    public CorredorData() { }

    public CorredorData(int idCarrera, int idCorredor, double velocidad, int checkpoint)
    {
        IdCarrera = idCarrera;
        IdCorredor = idCorredor;
        Velocidad = velocidad;
        UltimoCheckpoint = checkpoint;
    }
}

// Evento cuando finaliza una carrera
public record CarreraFinalizadaEvent(
    int IdCarrera, 
    DateTime? FechaFin, 
    int TotalCorredores, 
    int CorredoresFinalizados
);

// Evento cuando se inicia una carrera
public record CarreraIniciada(
    int IdCarrera, 
    List<int> IdCorredores, 
    List<PuntosDeControlDTO> TotalPuntosDeControl
);

// ⭐ NUEVO: Evento cuando un usuario se inscribe a una carrera
public record UsuarioInscritoEvent(
    int IdUsuario, 
    int IdCarrera,
    string Nombre,
    string Apellido,
    string Email
);