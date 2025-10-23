using ProyectoNET.Carreras.API.Models;
namespace ProyectoNET.Carreras.API.Models.Repositories
{
    public interface ICarreraRepository
    {
        Task<IEnumerable<Carrera>> GetAllAsync();
        Task<Carrera> GetByIdAsync(int id);
        Task AddAsync(Carrera carrera);
        Task UpdateAsync(Carrera carrera);
        Task DeleteAsync(Carrera carrera);
    }
}