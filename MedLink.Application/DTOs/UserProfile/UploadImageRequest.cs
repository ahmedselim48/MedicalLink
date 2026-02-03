using Microsoft.AspNetCore.Http;

namespace MedLink.Application.DTOs.UserProfile
{
    public class UploadImageRequest
    {
        public IFormFile Image { get; set; } = null!;
    }
}
