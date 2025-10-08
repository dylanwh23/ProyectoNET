// En: ProyectoNET.Shared/Contracts.cs
namespace ProyectoNET.Shared;

public record TiempoRegistrado(
    int IdCarrera,
    int IdCorredor,
    DateTime Tiempo,
    int PuntoDeControl
);