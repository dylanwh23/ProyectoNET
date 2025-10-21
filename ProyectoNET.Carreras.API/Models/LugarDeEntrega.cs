using System.ComponentModel.DataAnnotations;
namespace ProyectoNET.Carreras.API.Models;
public class LugarDeEntrega
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Nombre { get; set; }

    [Required]
    public int CarreraId { get; set; }
    public virtual Carrera Carrera { get; set; }
}