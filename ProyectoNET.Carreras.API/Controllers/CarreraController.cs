using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Data;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Shared;

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

    public CarreraController(
        ICarreraRepository carreraRepository,
        IParticipanteRepository participanteRepository,
        ILugarDeEntregaRepository lugarDeEntregaRepository,
        CarreraMapper mapper,
        IBlobStorageService blobStorageService,
        IBus bus,
        ILogger<CarreraController> logger)
    {
        _carreraRepository = carreraRepository;
        _participanteRepository = participanteRepository;
        _lugarDeEntregaRepository = lugarDeEntregaRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
        _bus = bus;
        _logger = logger;
    }

    [HttpPost("api/carreras")]
    public async Task<IActionResult> CrearCarrera([FromBody] CreateCarreraDTO request)
    {
        var carrera = _mapper.ToEntity(request);
        await _carreraRepository.AddAsync(carrera);
        return CreatedAtAction(nameof(ObtenerCarrera), new { id = carrera.Id }, carrera);
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
        var carreras = await _carreraRepository.GetAllAsync();
        var carrerasDTO = carreras.Select(c => _mapper.ToCarrerasListDTO(c));
        return Ok(carrerasDTO);
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
            var endpoint = await _bus.GetSendEndpoint(new Uri("queue:simulador-carreras"));
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

    // ✅ ENDPOINT DE PRUEBA - Para verificar que el consumer funciona
    [HttpPost("test/finalizar-carrera/{id}")]
    public async Task<IActionResult> TestFinalizarCarrera(int id)
    {
        try
        {
            var evento = new ProyectoNET.Shared.CarreraFinalizadaEvent
            {
                IdCarrera = id,
                FechaFin = DateTime.UtcNow,
                TotalCorredores = 3,
                CorredoresFinalizados = 3
            };

            await _bus.Publish(evento);

            return Ok(new { mensaje = $"Evento de finalización publicado para carrera {id}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

}