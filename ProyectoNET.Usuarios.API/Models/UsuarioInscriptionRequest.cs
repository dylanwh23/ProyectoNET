using System.ComponentModel.DataAnnotations;

namespace ProyectoNET.Usuarios.API.Models
{
    public class UsuarioInscriptionRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una carrera")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera válida")]
        public int CarreraId { get; set; }
    }
}