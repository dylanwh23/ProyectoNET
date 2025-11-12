using ProyectoNET.Shared;
using ProyectoNET.Shared.WebApp; // O donde estén tus DTOs
namespace ProyectoNET.Carreras.API.Services;
public interface ICarreraStateService
{
    // Método para crear la carrera en la caché
    void InicializarCarrera(CarreraIniciadaEvent evento);

    // Método para actualizar un corredor en la caché
    void ActualizarProgreso(ProgresoCorredorActualizado evento);

    // Método para que los nuevos clientes (F5) obtengan el estado
    (List<int> Corredores, List<PuntosDeControlDTO> Puntos, Dictionary<int, CarreraData> EstadoActual) GetEstadoActual(int carreraId);

    // Método para el broadcaster (ver Parte 2)
    List<int> GetCarrerasActivas();
    Dictionary<int, CarreraData> GetEstadoCorredores(int carreraId);
}