using Microsoft.EntityFrameworkCore;
using ProyectoNET.Carreras.API.Models;

namespace ProyectoNET.Carreras.API.Data;
public class CarrerasDbContext : DbContext
{
    public CarrerasDbContext(DbContextOptions<CarrerasDbContext> options)
        : base(options)
    {
    }
    public DbSet<Carrera> Carreras { get; set; }
    public DbSet<Participante> Participantes { get; set; }
    public DbSet<LugarDeEntrega> LugaresDeEntrega { get; set; }
}