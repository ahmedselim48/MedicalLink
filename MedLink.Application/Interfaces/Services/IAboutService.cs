using MedLink.Domain.Entities.Content;

namespace MedLink.Application.Interfaces.Services
{
    public interface IAboutService
    {
        Task<About?> GetAboutByIdAsync(int id);

        Task<About> AddAboutAsync(About about);
        Task UpdateAboutAsync(About about);
        Task DeleteAboutAsync(int id);
    }
}
