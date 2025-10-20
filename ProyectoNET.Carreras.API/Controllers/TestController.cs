using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Data;
namespace ProyectoNET.Carreras.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly CarrerasDbContext _context;

    public TestController(CarrerasDbContext context)
    {
        _context = context;
    }
    [HttpPost("crear-carrera")]
    public IActionResult CrearCarrera()
    {
        var carrera = new Models.Carrera
        {
            Nombre = "Maratón Ciudad de México",
            Descripcion = "Una maratón anual en la Ciudad de México.",
            Ubicacion = "Ciudad de México",
            FechaCreada = DateTime.UtcNow,
            LugaresRetiroEquipamiento = new List<string> { "Centro Deportivo", "Tienda Oficial" },
            CostoInscripcion = 500,
            CantidadMaximaParticipantes = 1000,
            Estado = "Pendiente"
        };

        _context.Carreras.Add(carrera);
        _context.SaveChanges();

        return Ok(new { Mensaje = "Carrera creada exitosamente", CarreraId = carrera.Id });
    }

}
