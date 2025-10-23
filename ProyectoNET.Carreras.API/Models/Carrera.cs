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
        public DateTime FechaCreada { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public virtual ICollection<LugarDeEntrega> LugaresRetiroEquipamiento { get; set; } = new HashSet<LugarDeEntrega>();
        public long CostoInscripcion { get; set; }
        public virtual ICollection<Participante> Participantes { get; set; } = new HashSet<Participante>();
        public int CantidadParticipantes => Participantes?.Count ?? 0;
        public int CantidadMaximaParticipantes { get; set; }
        public enum Estado { Pendiente, EnProgreso, Finalizada } 
        public Estado EstadoCarrera { get; set; } = Estado.Pendiente;

    }
}