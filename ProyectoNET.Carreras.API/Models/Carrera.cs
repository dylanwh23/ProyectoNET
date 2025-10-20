using System.ComponentModel.DataAnnotations;
namespace ProyectoNET.Carreras.API.Models
{
    public class Carrera
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Descripcion { get; set; }
        [Required]
        public string Ubicacion { get; set; }
        [Required]
        public DateTime FechaCreada { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        [Required]
        public List<string> LugaresRetiroEquipamiento { get; set; }
        [Required]
        public long CostoInscripcion { get; set; }
        public List<Participante> Participantes { get; set; }
        public int CantidadParticipantes => Participantes?.Count ?? 0;
        [Required]
        public int CantidadMaximaParticipantes { get; set; }
        public string Estado { get; set; } // Ejemplo: "Pendiente", "En Progreso", "Finalizada"

    }
}