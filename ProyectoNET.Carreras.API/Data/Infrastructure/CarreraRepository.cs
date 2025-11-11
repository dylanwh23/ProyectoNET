using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Data;
using Microsoft.EntityFrameworkCore;
public class CarreraRepository : ICarreraRepository
{
    private readonly CarrerasDbContext _context;
    public CarreraRepository(CarrerasDbContext context)
    {
        _context = context;
    }
    // Implementation of ICarreraRepository methods
    public async Task<IEnumerable<Carrera>> GetAllAsync()
    {
        return await _context.Carreras.ToListAsync();
    }
    public async Task<Carrera> GetByIdAsync(int id)
    {
        return await _context.Carreras.FindAsync(id);
    }
    public async Task AddAsync(Carrera carrera)
    {
        await _context.Carreras.AddAsync(carrera);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Carrera carrera)
    {
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Carrera carrera)
    {
        _context.Carreras.Remove(carrera);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Carrera>> GetCarerasEnCursoAsync()
    {
        return await _context.Carreras
            .Where(c => c.EstadoCarrera == Carrera.Estado.EnProgreso)
            .ToListAsync();
    }

    public async Task<IEnumerable<Carrera>> GetCarerasAbiertasInscripcionAsync()
    {
        return await _context.Carreras
            .Where(c => c.EstadoCarrera == Carrera.Estado.Pendiente)
            .ToListAsync();
    }
}