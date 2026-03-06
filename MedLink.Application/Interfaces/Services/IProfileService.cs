using MedLink.Application.DTOs.UserProfile;
using MedLink.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Services
{
    public interface IProfileService
    {
        Task<UserProfileDashboardDto> GetMyDashboardAsync(string userId);
        Task<UserProfile> GetProfileByUserIdAsync(string userId);
        Task<UserProfile> CreateAsync(string userId, string fullName);
        Task<EditProfileDto> GetMyProfileAsync(string userId);
        Task UpdateMyProfileAsync(string userId, UpdateProfileDto dto);
        Task UpdateProfileImageAsync(string userId, string imageUrl);
        Task RemoveProfileImageAsync(string userId);
    }

}
