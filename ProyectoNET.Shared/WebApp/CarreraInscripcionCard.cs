using System;
using System.Runtime.InteropServices;

namespace ProyectoNET.Shared.WebApp;

public class CarreraInscripcionCard
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string ImagenPromocional { get; set; } = string.Empty;
    public long CostoInscripcion { get; set; }
    public int CantidadMaximaParticipantes { get; set; }
    public int CantidadParticipantes { get; set; }
}
