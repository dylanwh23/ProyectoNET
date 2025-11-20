using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Añadir este using
using System.Net.Mail;
using System.Threading.Tasks;

namespace ProyectoNET.Usuarios.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["SmtpSettings:Host"];
            var smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
            var smtpUsername = _configuration["SmtpSettings:Username"];
            var smtpPassword = _configuration["SmtpSettings:Password"];
            var fromEmail = _configuration["SmtpSettings:FromEmail"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogError("⚠️ Configuración de SMTP incompleta. No se puede enviar el correo electrónico real.");
                _logger.LogInformation("📧 Correo simulado a {ToEmail} - Asunto: {Subject}, Cuerpo: {Body}", toEmail, subject, body);
                return;
            }

            try
            {
                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true; // La mayoría de los SMTP requieren SSL
                    client.Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false, // Puedes cambiarlo a true si el cuerpo es HTML
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("✅ Correo electrónico enviado exitosamente a {ToEmail}", toEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al enviar correo electrónico a {ToEmail}", toEmail);
                _logger.LogInformation("📧 Correo de respaldo simulado a {ToEmail} - Asunto: {Subject}, Cuerpo: {Body}", toEmail, subject, body);
            }
        }
    }
}
