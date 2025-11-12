// En ProyectoNET.Shared.WebApp (o donde viva CarreraData)
using ProyectoNET.WebApp;

public class CarreraData
{
    // (Tus propiedades existentes)
    public int CarreraId { get; set; }
    public int CorredorId { get; set; }
    public string Checkpoint { get; set; } = string.Empty;
    public double Velocidad { get; set; }
    public int TramosCompletados { get; set; }
    public float KmRecorridos { get; set; }
    public enum Estado { Pendiente, EnProgreso, Finalizada }
    public Estado EstadoCarrera { get; set; } = Estado.Pendiente;


    // Constructor vacío (necesario para SignalR/deserialización)
    public CarreraData() {}

    // ¡NUEVO! Constructor de copia
    public CarreraData(CarreraData source)
    {
        CarreraId = source.CarreraId;
        CorredorId = source.CorredorId;
        Checkpoint = source.Checkpoint;
        Velocidad = source.Velocidad;
        TramosCompletados = source.TramosCompletados;
        KmRecorridos = source.KmRecorridos;
        EstadoCarrera = Estado.EnProgreso;
    }
}
