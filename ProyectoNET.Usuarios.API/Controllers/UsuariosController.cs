using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNET.Usuarios.API.Data;
using ProyectoNET.Usuarios.API.Models;
using ProyectoNET.Shared.EventosRabbit;
using MassTransit;

namespace ProyectoNET.Usuarios.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<UsuariosController> _logger;
        private static readonly Random _random = new Random();

        public UsuariosController(
            UsuariosDbContext context, 
            IPublishEndpoint publishEndpoint,
            ILogger<UsuariosController> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        [HttpPost("inscripcion")]
        public async Task<IActionResult> InscribirUsuario([FromBody] UsuarioInscriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Verificar si el usuario ya existe por email
                var usuarioExistente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                string generatedPassword = GenerateRandomPassword();
                Usuario usuario;

                if (usuarioExistente == null)
                {
                    // Crear nuevo usuario
                    usuario = new Usuario
                    {
                        Nombre = request.Nombre,
                        Apellido = request.Apellido,
                        Email = request.Email,
                        PasswordHash = generatedPassword, // ⚠️ En producción, usar hash real (BCrypt)
                        FechaRegistro = DateTime.UtcNow
                    };
                    
                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ Nuevo usuario creado: {Email} (ID: {Id})", request.Email, usuario.Id);
                }
                else
                {
                    usuario = usuarioExistente;
                    _logger.LogInformation("ℹ️ Usuario existente encontrado: {Email} (ID: {Id})", request.Email, usuario.Id);
                }

                // Publicar evento de inscripción a RabbitMQ
                var usuarioInscritoEvent = new UsuarioInscritoEvent(
                    usuario.Id, 
                    request.CarreraId,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email
                );
                
                await _publishEndpoint.Publish(usuarioInscritoEvent);
                
                _logger.LogInformation("📨 Evento de inscripción publicado: Usuario {UserId} → Carrera {CarreraId}", 
                    usuario.Id, request.CarreraId);

                return Ok(new
                {
                    UserId = usuario.Id,
                    GeneratedPassword = generatedPassword,
                    Message = "Inscripción procesada exitosamente",
                    Email = usuario.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al procesar inscripción");
                return StatusCode(500, new { error = "Error al procesar la inscripción" });
            }
        }

        private string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[_random.Next(s.Length)]).ToArray());
        }
        
        // Endpoint de prueba
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Usuarios API funcionando correctamente", timestamp = DateTime.UtcNow });
        }
    }
}