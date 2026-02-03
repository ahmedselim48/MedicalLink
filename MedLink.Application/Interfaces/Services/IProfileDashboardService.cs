using MedLink.Application.DTOs.UserProfile;

namespace MedLink.Application.Interfaces.Services
{
    public interface IProfileDashboardService
    {
        Task<UserProfileDashboardDto> GetMyDashboardAsync(string userId);

    }
}
