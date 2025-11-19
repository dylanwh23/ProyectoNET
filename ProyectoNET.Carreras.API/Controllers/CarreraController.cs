using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Data;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Shared;
using ProyectoNET.Shared.AdminWebApp;
using ProyectoNET.Shared.EventosRabbit;

[ApiController]
[Route("")]
public class CarreraController : ControllerBase
{
    private readonly ICarreraRepository _carreraRepository;
    private readonly IParticipanteRepository _participanteRepository;
    private readonly ILugarDeEntregaRepository _lugarDeEntregaRepository;
    private readonly CarreraMapper _mapper;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IBus _bus;
    private readonly ILogger<CarreraController> _logger;
    private readonly CarrerasDbContext _context;

    public CarreraController(
        ICarreraRepository carreraRepository,
        IParticipanteRepository participanteRepository,
        ILugarDeEntregaRepository lugarDeEntregaRepository,
        CarreraMapper mapper,
        IBlobStorageService blobStorageService,
        IBus bus,
        ILogger<CarreraController> logger,
        CarrerasDbContext context)
    {
        _carreraRepository = carreraRepository;
        _participanteRepository = participanteRepository;
        _lugarDeEntregaRepository = lugarDeEntregaRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
        _bus = bus;
        _logger = logger;
        _context = context;
    }

    [HttpPost("api/carreras")]
    public async Task<IActionResult> CrearCarrera([FromBody] CreateCarreraDTO request)
    {
        try
        {
            // 1. Convertir DTO a Entidad
            var carrera = _mapper.ToEntity(request);

            // 2. Guardar en BD (EF Core asigna los IDs aquí)
            await _carreraRepository.AddAsync(carrera);

            // 3. ♻️ CONVERTIR A DTO ANTES DE DEVOLVER (Rompe el ciclo infinito)
            var carreraResponse = _mapper.ToDTO(carrera);

            // 4. Devolver el DTO, no la entidad
            return CreatedAtAction(nameof(ObtenerCarrera), new { id = carrera.Id }, carreraResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al crear carrera");
            // Esto te permite ver el error real en la consola del navegador
            return StatusCode(500, new { error = ex.Message, detalle = ex.InnerException?.Message });
        }
    }

    [HttpGet("api/carreras/{id}")]
    public async Task<IActionResult> ObtenerCarrera(int id)
    {
        var carrera = await _carreraRepository.GetByIdAsync(id);
        if (carrera == null)
        {
            return NotFound();
        }
        var carreraDTO = _mapper.ToDTO(carrera);
        return Ok(carreraDTO);
    }

    [HttpGet("api/carreras")]
    public async Task<IActionResult> ObtenerCarreras()
    {
        try
        {
            var carreras = await _context.Carreras
                .Include(c => c.LugaresRetiroEquipamiento) // ✅ AGREGAR ESTO
                .ToListAsync();

            var carrerasDTO = carreras.Select(c => _mapper.ToCarrerasListDTO(c));
            return Ok(carrerasDTO);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener carreras");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("api/carreras/en-curso")]
    public async Task<IActionResult> ObtenerCarrerasEnCurso()
    {
        var carreras = await _carreraRepository.GetCarerasEnCursoAsync();
        var carrerasDTO = carreras.Select(c => _mapper.ToCarreraEnCursoCard(c));
        return Ok(carrerasDTO);
    }

    [HttpGet("api/carreras/inscripcion")]
    public async Task<IActionResult> ObtenerCarrerasInscripcion()
    {
        var carreras = await _carreraRepository.GetCarerasAbiertasInscripcionAsync();
        var carrerasDTO = carreras.Select(c => _mapper.ToCarreraInscripcionCard(c));
        return Ok(carrerasDTO);
    }

    [HttpDelete("api/carreras/{id}")]
    public async Task<IActionResult> EliminarCarrera(int id)
    {
        var carrera = await _carreraRepository.GetByIdAsync(id);
        if (carrera == null)
        {
            return NotFound();
        }
        await _carreraRepository.DeleteAsync(carrera);
        return NoContent();
    }

    [HttpPut("api/carreras/{id}")]
    public async Task<IActionResult> ActualizarCarrera(int id, [FromBody] UpdateCarreraDTO request)
    {
        var carrera = await _carreraRepository.GetByIdAsync(id);
        if (carrera == null)
        {
            return NotFound();
        }
        _mapper.UpdateEntity(request, carrera);
        await _carreraRepository.UpdateAsync(carrera);
        return NoContent();
    }

    [HttpPost("api/carreras/{id}/imagen")]
    public async Task<IActionResult> SubirImagen(int id, IFormFile imagen)
    {
        var carrera = await _carreraRepository.GetByIdAsync(id);
        if (carrera == null)
        {
            return NotFound();
        }

        if (imagen == null || imagen.Length == 0)
        {
            return BadRequest("No se envió ninguna imagen.");
        }

        string urlDeImagen;

        await using (var stream = imagen.OpenReadStream())
        {
            urlDeImagen = await _blobStorageService.UploadAsync(
                stream,
                imagen.FileName,
                "default");
        }
        carrera.ImagenPromocional = urlDeImagen;
        await _carreraRepository.UpdateAsync(carrera);
        return Ok(new { UrlImagen = urlDeImagen });
    }

    // ✅ NUEVO ENDPOINT - Iniciar Carrera
    [HttpPost("carrera/iniciar")]
    public async Task<IActionResult> IniciarCarrera([FromBody] IniciarCarreraCommand command)
    {
        try
        {
            // 1️⃣ Obtener y validar la carrera
            var carrera = await _carreraRepository.GetByIdAsync(command.IdCarrera);

            if (carrera == null)
            {
                _logger.LogWarning($"⚠️ Carrera {command.IdCarrera} no encontrada");
                return NotFound(new { message = $"Carrera {command.IdCarrera} no encontrada" });
            }

            _logger.LogInformation($"🏁 Iniciando carrera {carrera.Id} - Estado actual: {carrera.EstadoCarrera}");

            // 2️⃣ ✅ ACTUALIZAR ESTADO A EN PROGRESO
            carrera.EstadoCarrera = Carrera.Estado.EnProgreso;
            carrera.FechaInicio = DateTime.UtcNow;

            await _carreraRepository.UpdateAsync(carrera);

            _logger.LogInformation($"✅ Estado actualizado a: {carrera.EstadoCarrera}");

            // 3️⃣ Enviar comando a la cola del simulador
            var endpoint = await _bus.GetSendEndpoint(new Uri("queue:IniciarCarrera"));
            await endpoint.Send(command);

            _logger.LogInformation($"📨 Comando enviado al simulador para carrera {command.IdCarrera}");

            return Accepted(new
            {
                message = $"Carrera {command.IdCarrera} iniciada y comando enviado al simulador.",
                carreraId = carrera.Id,
                estado = carrera.EstadoCarrera.ToString(),
                fechaInicio = carrera.FechaInicio
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error al iniciar carrera {command.IdCarrera}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ✅ BONUS: Endpoint para finalizar carrera manualmente (si lo necesitas)
    [HttpPost("carrera/{id}/finalizar")]
    public async Task<IActionResult> FinalizarCarrera(int id)
    {
        try
        {
            var carrera = await _carreraRepository.GetByIdAsync(id);

            if (carrera == null)
            {
                return NotFound($"Carrera {id} no encontrada");
            }

            carrera.EstadoCarrera = Carrera.Estado.Finalizada;
            carrera.FechaFin = DateTime.UtcNow;

            await _carreraRepository.UpdateAsync(carrera);

            _logger.LogInformation($"🏁 Carrera {id} finalizada");

            return Ok(new
            {
                message = "Carrera finalizada exitosamente",
                carreraId = id,
                estado = carrera.EstadoCarrera.ToString(),
                fechaFin = carrera.FechaFin
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al finalizar carrera {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ✅ Endpoint de prueba
    [HttpGet("carreras-test")]
    public IActionResult Test()
    {
        return Ok("Respuesta del Microservicio de Carreras");
    }


    [HttpGet("{id}/detalle")]
    public async Task<ActionResult<DetalleCarreraDto>> ObtenerDetalle(int id)
    {
        // ✅ CAMBIO CLAVE: Usar _carreraRepository en lugar de _context directo.
        // El repositorio ya tiene el .Include(...) configurado, así que traerá los lugares.
        var carrera = await _carreraRepository.GetByIdAsync(id);

        if (carrera == null)
            return NotFound();

        var detalle = new DetalleCarreraDto
        {
            Id = carrera.Id,
            Nombre = carrera.Nombre,
            Descripcion = carrera.Descripcion,
            Ubicacion = carrera.Ubicacion,
            FechaInicio = carrera.FechaInicio,
            FechaFin = carrera.FechaFin,
            CostoInscripcion = carrera.CostoInscripcion,
            CantidadMaximaParticipantes = carrera.CantidadMaximaParticipantes,
            Estado = carrera.EstadoCarrera.ToString(),
            ImagenUrl = carrera.ImagenPromocional,

            // Mapeo seguro de la lista
            LugaresRetiroEquipamiento = carrera.LugaresRetiroEquipamiento?
                .Select(l => l.Nombre)
                .ToList() ?? new()
        };

        return Ok(detalle);
    }



}