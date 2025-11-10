using System.ComponentModel.DataAnnotations;
namespace ProyectoNET.Carreras.API.Controllers.DTOs;

public record CreateCarreraDTO
{
    [Required]
    [MinLength(6)]
    [MaxLength(100)]
    public string Nombre { get; init; }
    [Required]
    [MinLength(10)]
    [MaxLength(500)]
    public string Descripcion { get; init; }
    [Required]
    [MinLength(10)]
    [MaxLength(100)]
    public string Ubicacion { get; init; }
    public DateTime? FechaInicio { get; init; }
    public long CostoInscripcion { get; init; }
    public int CantidadMaximaParticipantes { get; init; }
}