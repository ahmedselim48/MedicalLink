using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<UserProfileDashboardDto> GetMyDashboardAsync(string userId)
        {
            throw new NotImplementedException();
        }

       public async Task<UserProfile> GetProfileByUserIdAsync(string userId)
{
    // بنجيب كل البروفايلات ونختار اللي الـ UserId بتاعه مطابق
    var profiles = await _unitOfWork.Repository<UserProfile>().GetAllAsync();
    
    var profile = profiles.FirstOrDefault(u => u.UserId == userId);

    return profile;
}
    }
}
