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

    // ✅ CORREGIDO: Agregado .Include()
    public async Task<IEnumerable<Carrera>> GetAllAsync()
    {
        // Explicitly exclude Participantes for general GetAll to prevent serialization issues
        // if the DTOs don't need them directly.
        return await _context.Carreras
                             .Include(c => c.LugaresRetiroEquipamiento) // Include if needed by DTOs
                             .AsNoTracking() // Optional: if data is read-only
                             .ToListAsync();
    }

    // ✅ CORREGIDO: Cambiado FindAsync por FirstOrDefaultAsync + Include
    public async Task<Carrera> GetByIdAsync(int id)
    {
        return await _context.Carreras
            .Include(c => c.LugaresRetiroEquipamiento) // <--- ¡LA CLAVE DEL PROBLEMA!
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Carrera carrera)
    {
        await _context.Carreras.AddAsync(carrera);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Carrera carrera)
    {
        // Asegúrate de que la entidad esté "trackeada" o actualizada en el contexto
        _context.Carreras.Update(carrera);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Carrera carrera)
    {
        _context.Carreras.Remove(carrera);
        await _context.SaveChangesAsync();
    }

    // ✅ CORREGIDO: Agregado .Include()
    public async Task<IEnumerable<Carrera>> GetCarerasEnCursoAsync()
    {
        return await _context.Carreras
            .Include(c => c.LugaresRetiroEquipamiento)
            .Where(c => c.EstadoCarrera == Carrera.Estado.EnProgreso)
            .Include(c => c.LugaresRetiroEquipamiento) // Include if needed by DTOs
            .AsNoTracking()
            .ToListAsync();
    }

    // ✅ CORREGIDO: Agregado .Include()
    public async Task<IEnumerable<Carrera>> GetCarerasAbiertasInscripcionAsync()
    {
        return await _context.Carreras
            .Include(c => c.LugaresRetiroEquipamiento)
            .Where(c => c.EstadoCarrera == Carrera.Estado.Pendiente)
            .Include(c => c.LugaresRetiroEquipamiento) // Include if needed by DTOs
            .AsNoTracking()
            .ToListAsync();
    }
}
