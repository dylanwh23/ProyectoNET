using ProyectoNET.Carreras.API.Models;
namespace ProyectoNET.Carreras.API.Models.Repositories
{
    public interface ILugarDeEntregaRepository
    {
        Task<IEnumerable<LugarDeEntrega>> GetAllAsync();
        Task<LugarDeEntrega> GetByIdAsync(int id);
        Task AddAsync(LugarDeEntrega lugarDeEntrega);
        Task UpdateAsync(LugarDeEntrega lugarDeEntrega);
        Task DeleteAsync(LugarDeEntrega lugarDeEntrega);
    }
}