using MassTransit;
using ProyectoNET.Carreras.API.Data;
using ProyectoNET.Carreras.API.Models;
using ProyectoNET.Shared.EventosRabbit;
using Microsoft.EntityFrameworkCore; // Needed for FirstOrDefaultAsync, Include

namespace ProyectoNET.Carreras.API.Consumers
{
    public class UsuarioInscritoEventConsumer : IConsumer<UsuarioInscritoEvent>
    {
        private readonly CarrerasDbContext _context;
        private readonly ILogger<UsuarioInscritoEventConsumer> _logger;

        public UsuarioInscritoEventConsumer(CarrerasDbContext context, ILogger<UsuarioInscritoEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UsuarioInscritoEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received UsuarioInscritoEvent for UserId: {UserId}, CarreraId: {CarreraId}", message.IdUsuario, message.IdCarrera);

            var carrera = await _context.Carreras
                                        .Include(c => c.Participantes)
                                        .FirstOrDefaultAsync(c => c.Id == message.IdCarrera);

            if (carrera == null)
            {
                _logger.LogWarning("Carrera with ID {CarreraId} not found. Cannot create participant.", message.IdCarrera);
                return;
            }

            if (carrera.Participantes.Count >= carrera.CantidadMaximaParticipantes)
            {
                _logger.LogWarning("Carrera with ID {CarreraId} is full. Cannot create participant for UserId: {UserId}", message.IdCarrera, message.IdCarrera);
                // Potentially publish a "CarreraLlenaEvent" or similar
                return;
            }

            // Check if this user is already a participant in this race to prevent duplicates
            if (carrera.Participantes.Any(p => p.UserId == message.IdUsuario))
            {
                _logger.LogWarning("⚠️  INSCRIPCIÓN DUPLICADA DETECTADA: Usuario {UserId} ya está inscrito en Carrera {CarreraId}. Saltando inscripción.", message.IdUsuario, message.IdCarrera);
                return;
            }

            _logger.LogInformation("✅ Creando nuevo participante para UserId: {UserId} en CarreraId: {CarreraId}", message.IdUsuario, message.IdCarrera);
            var participante = new Participante
            {
                UserId = message.IdUsuario,
                CarreraId = message.IdCarrera,
                FechaInscripcion = DateTime.UtcNow,
                IsEquipamientoEntregado = false
            };

            carrera.Participantes.Add(participante);
            carrera.CantidadParticipantes++; // Assuming this property is updated manually

            await _context.SaveChangesAsync();
            _logger.LogInformation("Participante created successfully for UserId: {UserId} in CarreraId: {CarreraId}", message.IdUsuario, message.IdCarrera);
        }
    }
}
