using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Specifications;
using MedLink.Domain.Entities.Content;

namespace MedLink.Application.Interfaces.Services
{
    public interface ILanguageService
    {
        Task<Language?> GetLanguageByIdAsync(int id);
        Task<IReadOnlyList<Language>> GetAllLanguagesAsync(ISpecification<Language>? spec = null);
        Task<Language> AddLanguageAsync(Language language);
        Task UpdateLanguageAsync(Language language);
        Task DeleteLanguageAsync(int id);
        IReadOnlyList<LanguageDto> GetAllLanguages();
    }
}
