using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoNET.Usuarios.API.Data;
using ProyectoNET.Usuarios.API.Models;
using ProyectoNET.Shared.EventosRabbit;
using ProyectoNET.Usuarios.API.Services; // Añadir este using
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
        private readonly IEmailService _emailService; // Inyectar IEmailService
        private static readonly Random _random = new Random();

        public UsuariosController(
            UsuariosDbContext context, 
            IPublishEndpoint publishEndpoint,
            ILogger<UsuariosController> logger,
            IEmailService emailService) // Añadir a la inyección de dependencias
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _emailService = emailService; // Asignar
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

                if (usuarioExistente != null)
                {
                    _logger.LogInformation("ℹ️ Intento de inscripción con email existente: {Email}. Se sugiere iniciar sesión para ingresar a carreras.", request.Email);
                    return Conflict(new { error = "Logeate para ingresar a carreras." });
                }
                
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
                
                // Enviar la contraseña por correo electrónico
                var emailSubject = "Tu contraseña para ProyectoNET";
                var emailBody = $"Hola {usuario.Nombre},\n\nTu cuenta ha sido creada exitosamente. Tu contraseña es: {generatedPassword}\n\nPor favor, inicia sesión y considera cambiar tu contraseña.\n\nSaludos,\nEl equipo de ProyectoNET";
                await _emailService.SendEmailAsync(usuario.Email, emailSubject, emailBody);
                
                return Ok(new
                {
                    UserId = usuario.Id,
                    Message = "Inscripción procesada exitosamente. La contraseña ha sido enviada a tu correo electrónico.",
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
