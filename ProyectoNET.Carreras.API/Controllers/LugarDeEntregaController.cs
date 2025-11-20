using Microsoft.AspNetCore.Mvc;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Carreras.API.Models.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProyectoNET.Carreras.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LugarDeEntregaController : ControllerBase
    {
        private readonly ILugarDeEntregaRepository _lugarDeEntregaRepository;

        public LugarDeEntregaController(ILugarDeEntregaRepository lugarDeEntregaRepository)
        {
            _lugarDeEntregaRepository = lugarDeEntregaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LugarDeEntrega>>> GetLugaresDeEntrega()
        {
            var lugares = await _lugarDeEntregaRepository.GetAllAsync();
            return Ok(lugares);
        }

        [HttpGet("porcarrera/{carreraId}")]
        public async Task<ActionResult<IEnumerable<LugarDeEntrega>>> GetLugaresDeEntregaPorCarrera(int carreraId)
        {
            var lugares = await _lugarDeEntregaRepository.GetAllAsync(); // Asumo que GetAllAsync trae todos y se filtra en memoria
            var lugaresFiltrados = lugares.Where(l => l.CarreraId == carreraId);
            return Ok(lugaresFiltrados);
        }

        // Otros endpoints CRUD si fueran necesarios
        [HttpGet("{id}")]
        public async Task<ActionResult<LugarDeEntrega>> GetLugarDeEntrega(int id)
        {
            var lugar = await _lugarDeEntregaRepository.GetByIdAsync(id);
            if (lugar == null)
            {
                return NotFound();
            }
            return Ok(lugar);
        }

        [HttpPost]
        public async Task<ActionResult<LugarDeEntrega>> PostLugarDeEntrega(LugarDeEntrega lugarDeEntrega)
        {
            await _lugarDeEntregaRepository.AddAsync(lugarDeEntrega);
            return CreatedAtAction(nameof(GetLugarDeEntrega), new { id = lugarDeEntrega.Id }, lugarDeEntrega);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLugarDeEntrega(int id, LugarDeEntrega lugarDeEntrega)
        {
            if (id != lugarDeEntrega.Id)
            {
                return BadRequest();
            }
            await _lugarDeEntregaRepository.UpdateAsync(lugarDeEntrega);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLugarDeEntrega(int id)
        {
            var lugarDeEntrega = await _lugarDeEntregaRepository.GetByIdAsync(id);
            if (lugarDeEntrega == null)
            {
                return NotFound();
            }
            await _lugarDeEntregaRepository.DeleteAsync(lugarDeEntrega);
            return NoContent();
        }
    }
}
