using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.User;

namespace MedLink.Application.Services
{
    public class UserLanguageService : IUserLanguageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserLanguageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<LanguageResponsetDto> GetPreferredLanguageAsync(string userId)
        {
            var profile = await _unitOfWork.Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            return new LanguageResponsetDto
            {
                PreferredLanguage = profile.PreferredLanguage
            };

        }

        public async Task UpdatePreferredLanguageAsync(string userId, UpdateLanguageDto dto)
        {
            var allowedLanguages = new[] { "en", "ar", "fr", "de", "es", "ja", "zh", "ru" };

            if (!allowedLanguages.Contains(dto.Language))
                throw new Exception("Invalid language");

            var profile = await _unitOfWork.Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            profile.PreferredLanguage = dto.Language;

            await _unitOfWork.Complete();
        }
    }
}
