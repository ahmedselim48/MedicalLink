using Microsoft.AspNetCore.Http;

namespace MedLink.Application.DTOs.UserProfile
{
    public class UpdateProfilePhotoDto
    {
        public IFormFile Photo { get; set; } = null!;
    }
}
