using ProyectoNET.Carreras.API.Models;

namespace ProyectoNET.Carreras.API.Models
{
    public class Participante
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public bool EquipamientoEntregado { get; set; }
        public Carrera Carrera { get; set; }
        public string LugarRetiroEquipamientoElegido { get; set; }

    }
}
