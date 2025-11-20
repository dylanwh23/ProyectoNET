using Microsoft.EntityFrameworkCore;
using ProyectoNET.Carreras.API.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json; 

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // ... (Otras configuraciones)

        modelBuilder.Entity<Carrera>(entity =>
        {
            entity.Property(c => c.Checkpoints)
                // 1. Definir cómo convertir el Dictionary a string (DB)
                .HasConversion(
                    // Value to Provider (C# -> DB): Serializar a JSON string
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    // Provider to Value (DB -> C#): Deserializar de JSON string a Dictionary
                    v => JsonSerializer.Deserialize<Dictionary<int, double>>(v, (JsonSerializerOptions?)null)
                )
                // ✅ CLAVE POSTGRES: Usar el tipo de columna JSONB
                .HasColumnType("jsonb"); 

            // 2. Definir cómo EF Core debe trackear los cambios
            entity.Property(c => c.Checkpoints)
                .Metadata.SetValueComparer(
                    new ValueComparer<Dictionary<int, double>>(
                        (c1, c2) => c1!.SequenceEqual(c2!),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                    )
                );
        });

        // ... (Fin de otras configuraciones)
    }

}