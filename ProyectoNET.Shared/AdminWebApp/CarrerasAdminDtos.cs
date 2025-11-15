using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.ComponentModel.DataAnnotations;

namespace ProyectoNET.Shared.AdminWebApp;

// DTO para listar carreras (lo que devuelve GET /api/Carreras)
public class CarreraDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string ImagenPromocional { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public int TotalCorredores { get; set; }
    public int TotalPuntosControl { get; set; }
    public decimal? Distancia { get; set; }
    public DateTime? FechaInicio { get; set; }
}

// DTO para crear carrera
public class CrearCarreraDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es requerida")]
    [MinLength(10, ErrorMessage = "Mínimo 10 caracteres")]
    [MaxLength(500, ErrorMessage = "Máximo 500 caracteres")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ubicación es requerida")]
    [MinLength(10, ErrorMessage = "Mínimo 10 caracteres")]
    [MaxLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public string Ubicacion { get; set; } = string.Empty;

    public DateTime? FechaInicio { get; set; }
    public long CostoInscripcion { get; set; }
    public int CantidadMaximaParticipantes { get; set; }
}

// DTO para iniciar carrera
public class IniciarCarreraDto
{
    public int IdCarrera { get; set; }
    public List<int> IdCorredores { get; set; } = new();
    public List<PuntoControlInicioDto> TotalPuntosDeControl { get; set; } = new();
}

public class PuntoControlInicioDto
{
    public int IdPuntoDeControl { get; set; }
    public float Km { get; set; }
}