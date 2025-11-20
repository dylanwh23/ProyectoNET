using System.Threading.Tasks;

namespace ProyectoNET.Usuarios.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
