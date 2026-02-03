using MedLink.Application.DTOs.UserProfile;

namespace MedLink.Application.Interfaces.Services
{
    public interface IUserLanguageService
    {
        Task<LanguageResponsetDto> GetPreferredLanguageAsync(string userId);
        Task UpdatePreferredLanguageAsync(string userId, UpdateLanguageDto dto);
    }
}
