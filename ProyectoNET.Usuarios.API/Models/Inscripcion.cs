using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoNET.Usuarios.API.Models
{
    public class Inscripcion
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int CarreraId { get; set; } // ID de la carrera del servicio de carreras

        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;
    }
}
