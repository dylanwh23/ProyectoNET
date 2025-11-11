using System;

namespace ProyectoNET.Shared;

public class CarreraCard
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public string ImagenPromocional { get; set; } = string.Empty;
}
