using Microsoft.EntityFrameworkCore;
//using ProyectoNET.Usuarios.API.Models;

namespace ProyectoNET.Usuarios.API.Data;
public class UsuariosDbContext : DbContext
{
    public UsuariosDbContext(DbContextOptions<UsuariosDbContext> options)
        : base(options)
    {
    }
    //public DbSet<Usuario> Usuarios { get; set; }
}