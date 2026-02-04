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

    }
}
