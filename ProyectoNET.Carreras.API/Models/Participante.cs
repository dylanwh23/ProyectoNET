using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Needed for ForeignKey attribute

namespace ProyectoNET.Carreras.API.Models
{
    public class Participante
    {
        [Key]
        public int Id { get; set; }
        
        // Reference to the User in Usuarios.API
        [Required]
        public int UserId { get; set; } 
        // Note: There won't be a direct navigation property to Usuario here, 
        // as Usuario is in a different API/microservice.

        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow; // Set default to current UTC time
        public bool IsEquipamientoEntregado { get; set; } = false; // Set default value

        [Required]
        public int CarreraId { get; set; }
        public virtual Carrera Carrera { get; set; }

        public virtual LugarDeEntrega? LugarRetiroEquipamientoElegido { get; set; } // Make nullable
        public int? IdLugarRetiroEquipamientoElegido { get; set; } // Make nullable and add foreign key attribute if applicable
    }
}
