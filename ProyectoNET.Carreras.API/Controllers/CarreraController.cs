using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Controllers.DTOs;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Shared.EventosRabbit;
using ProyectoNET.Carreras.API.Services;


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
    private readonly IGeoProcessingService _geoProcessingService;

    public CarreraController(
        ICarreraRepository carreraRepository,
        IParticipanteRepository participanteRepository,
        ILugarDeEntregaRepository lugarDeEntregaRepository,
        CarreraMapper mapper,
        IBlobStorageService blobStorageService,
        IBus bus,
        ILogger<CarreraController> logger,
        IGeoProcessingService geoProcessingService)
    {
        _carreraRepository = carreraRepository;
        _participanteRepository = participanteRepository;
        _lugarDeEntregaRepository = lugarDeEntregaRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
        _bus = bus;
        _logger = logger;
        _geoProcessingService = geoProcessingService;
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


    // --- LÓGICA DE GEOPROCESAMIENTO EN SUBIR RUTA ---

    [HttpPost("api/carreras/{id}/ruta")]
    public async Task<IActionResult> SubirRuta(int id, IFormFile archivoRuta)
    {
        // 1. Validaciones básicas
        if (archivoRuta == null || archivoRuta.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        if (!archivoRuta.FileName.EndsWith(".geojson") && !archivoRuta.FileName.EndsWith(".json"))
            return BadRequest("El archivo debe ser un GeoJSON (.json o .geojson).");

        var carrera = await _carreraRepository.GetByIdAsync(id);
        if (carrera == null) return NotFound($"Carrera {id} no encontrada");

        try
        {
            string contenidoGeoJson;
            // 2. Leer el contenido del archivo como texto
            using (var reader = new StreamReader(archivoRuta.OpenReadStream()))
            {
                contenidoGeoJson = await reader.ReadToEndAsync();
            }

            // 3. PROCESAR EL GEOJSON (CÁLCULO GEOMÉTRICO)
            var (checkpointsKm, totalDistanceKm) =
                await _geoProcessingService.CalculateCheckpointsAndDistanceAsync(contenidoGeoJson, id);

            // 4. Guardar en la base de datos
            carrera.RutaGeoJson = contenidoGeoJson;
            // ✅ CORREGIDO: Asignamos el Dictionary<int, double> directamente a la propiedad del modelo
            // Esto asume que EF Core (con Value Converter o mapeo nativo JSON) manejará la persistencia.
            carrera.Checkpoints = checkpointsKm;
            // Si la entidad Carrera tiene una propiedad para guardar la distancia total:
            carrera.Kms = totalDistanceKm;

            await _carreraRepository.UpdateAsync(carrera);

            _logger.LogInformation($"📍 Ruta GeoJSON actualizada para la carrera {id}. Distancia calculada: {totalDistanceKm} Km. Checkpoints: {checkpointsKm.Count}");

            return Ok(new
            {
                message = "Ruta cargada y validada correctamente",
                totalKmCalculado = totalDistanceKm,
                checkpointsDetectados = checkpointsKm.Count
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"❌ Error de datos GeoJSON al subir ruta para carrera {id}: {ex.Message}");
            return BadRequest(new { error = $"Error en el contenido GeoJSON: {ex.Message}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al subir ruta para carrera {id}");
            return StatusCode(500, new { error = ex.Message });
        }
    }


    // --- LÓGICA DE INICIO DE CARRERA ---

    [HttpPost("carrera/iniciar")]
    public async Task<IActionResult> IniciarCarrera([FromBody] IniciarCarreraRequest command)
    {
        try
        {
            // 1️⃣ Obtener y validar la carrera
            // El repositorio obtiene la carrera y EF Core automáticamente deserializa la columna de Checkpoints a Dictionary<int, double>
            var carrera = await _carreraRepository.GetByIdAsync(command.IdCarrera);

            if (carrera == null)
            {
                _logger.LogWarning($"⚠️ Carrera {command.IdCarrera} no encontrada");
                return NotFound(new { message = $"Carrera {command.IdCarrera} no encontrada" });
            }

            // Usamos la propiedad Checkpoints directamente (ya es Dictionary<int, double>)
            if (string.IsNullOrEmpty(carrera.RutaGeoJson) || carrera.Checkpoints == null || !carrera.Checkpoints.Any())
            {
                return BadRequest(new { message = "No se puede iniciar la carrera porque no tiene una ruta GeoJSON Y/O los checkpoints calculados. Asegúrese de haber subido la ruta previamente." });
            }

            if (carrera.EstadoCarrera != Carrera.Estado.Pendiente)
            {
                return BadRequest(new { message = $"Carrera {command.IdCarrera} ya está en estado {carrera.EstadoCarrera}." });
            }

            // 2️⃣ Actualizar estado
            carrera.EstadoCarrera = Carrera.Estado.EnProgreso;
            carrera.FechaInicio = DateTime.UtcNow;
            await _carreraRepository.UpdateAsync(carrera);

            _logger.LogInformation($"✅ Estado actualizado a: {carrera.EstadoCarrera}");

            // 3️⃣ Publicar el comando de inicio de simulación (MassTransit)
            //var corredores = carrera.Participantes.Select(p => p.Id).ToList();

            // ✅ Usamos inicializadores de propiedades para asignar los valores y evitar errores de constructor
            // La propiedad 'Checkpoints' ya es Dictionary<int, double>
            var iniciarCommand = new IniciarCarreraCommand
            (command.IdCarrera,
                command.IdCorredores,
                carrera.Checkpoints,
                carrera.RutaGeoJson);



            await _bus.Publish(iniciarCommand);

            _logger.LogInformation($"📨 Comando IniciarCarreraCommand publicado para carrera {command.IdCarrera}");

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



    // Dentro de CarrerasController.cs (o similar)

    [HttpGet("api/carrera/{id}/route")] // Ejemplo de endpoint: /api/carreras/1/route
    public async Task<ActionResult<string>> GetRaceRoute(int id)
    {
        // 1. Usa tu DbContext para buscar la carrera
        var carrera = await _carreraRepository.GetByIdAsync(id);


        if (carrera == null)
        {
            return NotFound($"No se encontró la carrera con ID {id}.");
        }

        // 2. CRÍTICO: Asegurarse de que el string no esté null/vacío
        if (string.IsNullOrEmpty(carrera.RutaGeoJson))
        {
            // Puedes devolver un 204 No Content o un JSON vacío válido si lo prefieres
            return NotFound("La ruta GeoJSON para esta carrera no está definida.");
        }

        // 3. Devolver el GeoJSON (el contenido del string)
        // Usamos Content() para asegurar que la respuesta sea solo el string de texto
        return Content(carrera.RutaGeoJson, "application/json");
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

            if (carrera.EstadoCarrera == Carrera.Estado.Finalizada)
            {
                return BadRequest($"Carrera {id} ya está finalizada.");
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
}