using MedLink.Application.DTOs.Identity;

namespace MedLink.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);

    }
}
