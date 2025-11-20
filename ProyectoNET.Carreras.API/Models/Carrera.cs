using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // Add this for [JsonIgnore]

namespace ProyectoNET.Carreras.API.Models
{
    public class Carrera
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        public string Descripcion { get; set; } = string.Empty;
        [Required]
        public string Ubicacion { get; set; } = string.Empty; // <-- Solución aquí
        public DateTime FechaCreada { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public virtual ICollection<LugarDeEntrega> LugaresRetiroEquipamiento { get; set; } = new HashSet<LugarDeEntrega>();
        public long CostoInscripcion { get; set; }
        [JsonIgnore] // Ignore Participantes during serialization unless explicitly included
        public virtual ICollection<Participante> Participantes { get; set; } = new HashSet<Participante>();
        public int CantidadParticipantes { get; set; }
        public int CantidadMaximaParticipantes { get; set; }
        public enum Estado { Pendiente, EnProgreso, Finalizada }
        public Estado EstadoCarrera { get; set; } = Estado.Pendiente;
        public string ImagenPromocional { get; set; } = "//127.0.0.1:10000/devstoreaccount1/default/carreradefault.png";
    }
}
