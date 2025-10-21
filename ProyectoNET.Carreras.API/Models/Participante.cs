using System.ComponentModel.DataAnnotations;
using ProyectoNET.Carreras.API.Models;

namespace ProyectoNET.Carreras.API.Models
{
    public class Participante
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        [Required]
        public string Email { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public bool IsEquipamientoEntregado { get; set; }

        [Required]
        public int CarreraId { get; set; }
        public virtual Carrera Carrera { get; set; }
        public virtual LugarDeEntrega LugarRetiroEquipamientoElegido { get; set; }
        [Required]
        public int IdLugarRetiroEquipamientoElegido { get; set; }
    }
}
