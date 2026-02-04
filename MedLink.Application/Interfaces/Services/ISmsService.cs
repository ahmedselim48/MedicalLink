using Twilio.Rest.Api.V2010.Account;

namespace MedLink.Application.Interfaces.Services
{
    public interface ISmsService
    {
        Task<MessageResource> SendSmsAsync(string to, string message);
        Task<string> SendVerificationTokenAsync(string phoneNumber);
        Task<bool> VerifyTokenAsync(string phoneNumber, string token);
    }
}
