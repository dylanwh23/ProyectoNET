using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Models.Repositories;
using ProyectoNET.Carreras.API.Mappers;
using ProyectoNET.Carreras.API.Controllers.DTOs;
public class CarreraController : ControllerBase
{
    private readonly ICarreraRepository _carreraRepository;
    private readonly IParticipanteRepository _participanteRepository;
    private readonly ILugarDeEntregaRepository _lugarDeEntregaRepository;
    private readonly CarreraMapper _mapper;

    private readonly IBlobStorageService _blobStorageService;

    public CarreraController(
        ICarreraRepository carreraRepository,
        IParticipanteRepository participanteRepository,
        ILugarDeEntregaRepository lugarDeEntregaRepository,
        CarreraMapper mapper,
        IBlobStorageService blobStorageService)
    {
        _carreraRepository = carreraRepository;
        _participanteRepository = participanteRepository;
        _lugarDeEntregaRepository = lugarDeEntregaRepository;
        _mapper = mapper;
        _blobStorageService = blobStorageService;
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
        var carrerasDTO = carreras.Select(c => _mapper.ToCarrerasDTO(c));
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

}