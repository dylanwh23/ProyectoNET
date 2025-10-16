// Models/InscripcionModel.cs

using System.ComponentModel.DataAnnotations;

public class InscripcionModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres.")]
    public string? Nombres { get; set; }

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres.")]
    public string? Apellidos { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime? FechaNacimiento { get; set; } = DateTime.Today.AddYears(-18);

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar una carrera.")]
    public int IdCarreraSeleccionada { get; set; }
}