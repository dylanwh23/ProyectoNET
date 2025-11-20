using System;
using System.Collections.Generic;

namespace ProyectoNET.Shared.EventosRabbit;

public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, Dictionary<int, double> TotalPuntosDeControl, string RutaGeoJson);
public record CarreraFinalizadaEvent(int IdCarrera, DateTime? FechaFin, int TotalCorredores, int CorredoresFinalizados);
public record CarreraIniciada(int IdCarrera, List<CorredorData> IdCorredores, Dictionary<int, double> TotalPuntosDeControl);

public record CorredorData
{
    public int IdCarrera { get; init; }
    public int IdCorredor { get; init; }
    public double Velocidad { get; init; }
    public int UltimoCheckpoint { get; init; }
    public double Latitud { get; init; }
    public double Longitud { get; init; }
    
    public double Km { get; init; }
    public TimeSpan TiempoOficial { get; init; }

    public TimeSpan TiempoNeto { get; init; }

    public CorredorData() { }

    public CorredorData(
        int idCarrera, 
        int idCorredor, 
        double velocidad, 
        int checkpoint, 
        double latitud, 
        double longitud,
        double km,              
        TimeSpan tiempoOficial,
        TimeSpan tiempoNeto
    )
    {
        IdCarrera = idCarrera;
        IdCorredor = idCorredor;
        Velocidad = velocidad;
        UltimoCheckpoint = checkpoint;
        Latitud = latitud;
        Longitud = longitud;
        Km = km;
        TiempoOficial = tiempoOficial;
        TiempoNeto = tiempoNeto;
    }
}


// ⭐ NUEVO: Evento cuando un usuario se inscribe a una carrera
public record UsuarioInscritoEvent(
    int IdUsuario, 
    int IdCarrera,
    string Nombre,
    string Apellido,
    string Email
);
