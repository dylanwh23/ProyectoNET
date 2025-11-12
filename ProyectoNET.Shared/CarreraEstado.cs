// Puedes crear este archivo, por ejemplo: CarreraEstado.cs
using ProyectoNET.Shared;

namespace ProyectoNET.WebApp;

public class CarreraEstado
{
    // 1. Los datos de configuración
    public bool CarreraIniciada { get; set; } = false;
    public List<PuntosDeControlDTO> PuntosDeControl { get; set; } = new();
    public float KmTotales { get; set; } = 0;

    // 2. El estado en tiempo real
    public Dictionary<int, CarreraData> EstadoCorredores { get; set; } = new();
}