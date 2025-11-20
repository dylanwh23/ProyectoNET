using System.ComponentModel.DataAnnotations;

namespace ProyectoNET.WebApp.Models
{
   public class InscripcionCarreraViewModel
{
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; } = DateTime.Today;

    public int LugarDeEntregaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera válida")]
    public int CarreraId { get; set; }
}

    public class CarreraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string ImagenPromocional { get; set; } = "http://127.0.0.1:10000/devstoreaccount1/default/carreradefault.png"; // Add this property
    }
}
