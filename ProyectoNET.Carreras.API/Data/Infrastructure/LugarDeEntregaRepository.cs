using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Data;
using Microsoft.EntityFrameworkCore;
public class LugarDeEntregaRepository : ILugarDeEntregaRepository
{
    private readonly CarrerasDbContext _context;

    public LugarDeEntregaRepository(CarrerasDbContext context)
    {
        _context = context;
    }

    // Implementation of ILugarDeEntregaRepository methods
    public async Task<IEnumerable<LugarDeEntrega>> GetAllAsync()
    {
        return await _context.LugaresDeEntrega.ToListAsync();
    }
    public async Task<LugarDeEntrega> GetByIdAsync(int id)
    {
        return await _context.LugaresDeEntrega.FindAsync(id);
    }
    public async Task AddAsync(LugarDeEntrega lugarDeEntrega)
    {
        await _context.LugaresDeEntrega.AddAsync(lugarDeEntrega);
    }
    public async Task UpdateAsync(LugarDeEntrega lugarDeEntrega)
    {
        _context.LugaresDeEntrega.Update(lugarDeEntrega);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(LugarDeEntrega lugarDeEntrega)
    {
        _context.LugaresDeEntrega.Remove(lugarDeEntrega);
        await _context.SaveChangesAsync();
    }
}