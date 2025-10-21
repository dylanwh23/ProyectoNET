using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Data;
using Microsoft.EntityFrameworkCore;
public class ParticipanteRepository : IParticipanteRepository
{
    private readonly CarrerasDbContext _context;

    public ParticipanteRepository(CarrerasDbContext context)
    {
        _context = context;
    }
    // Implementation of IParticipanteRepository methods
    public async Task<IEnumerable<Participante>> GetAllAsync()
    {
        return await _context.Participantes.ToListAsync();
    }
    public async Task<Participante> GetByIdAsync(int id)
    {
        return await _context.Participantes.FindAsync(id);
    }
    public async Task AddAsync(Participante participante)
    {
        await _context.Participantes.AddAsync(participante);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Participante participante)
    {
        _context.Participantes.Update(participante);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Participante participante)
    {
        _context.Participantes.Remove(participante);
        await _context.SaveChangesAsync();
    }
 
}