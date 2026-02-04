using Microsoft.AspNetCore.Http;

namespace MedLink.Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> UploadAsync(IFormFile file);
        Task DeleteAsync(string imageUrl);
    }
}
