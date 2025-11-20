using System.ComponentModel.DataAnnotations;

namespace ProyectoNET.Usuarios.API.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Apellido { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
