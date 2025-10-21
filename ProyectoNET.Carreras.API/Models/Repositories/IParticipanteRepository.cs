using ProyectoNET.Carreras.API.Models;
namespace ProyectoNET.Carreras.API.Models.Repositories
{
    public interface IParticipanteRepository
    {
        Task<IEnumerable<Participante>> GetAllAsync();
        Task<Participante> GetByIdAsync(int id);
        Task AddAsync(Participante participante);
        Task UpdateAsync(Participante participante);
        Task DeleteAsync(Participante participante);
    }
}