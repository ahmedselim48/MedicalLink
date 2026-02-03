using MedLink.Application.DTOs.UserProfile;

namespace MedLink.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task CreateAsync(string userId, string fullName);
        Task<EditProfileDto> GetMyProfileAsync(
            string userId);

        Task UpdateMyProfileAsync(string userId, UpdateProfileDto dto);

        Task UpdateProfileImageAsync(string userId, string imageUrl);

        Task RemoveProfileImageAsync(string userId);

    }

}
