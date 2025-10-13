using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProyectoNET.Shared;



// DTO de punto de control
public record PuntosDeControlDTO(int IdPuntoDeControl, float Km);

// Comando para iniciar carrera
public record IniciarCarreraCommand(int IdCarrera, List<int> IdCorredores, List<PuntosDeControlDTO> TotalPuntosDeControl);

// DTO de tiempo por tramo
public record TiempoPorTramoDTO(int DesdePuntoDeControlId, int HastaPuntoDeControlId, TimeSpan Tiempo);

// Evento de progreso del corredor
public record ProgresoCorredorActualizado(
    int IdCarrera,
    int IdCorredor,
    PuntosDeControlDTO UltimoCheckpointPasado,
    float VelocidadKmh,
    List<TiempoPorTramoDTO> TiemposPorTramo
);

