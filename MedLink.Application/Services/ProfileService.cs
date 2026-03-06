using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace MedLink.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public async Task<UserProfile> CreateAsync(string userId, string fullName)
        {
            var profile = new UserProfile
            {
                UserId = userId,
                FullName = fullName
            };

            await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            await _unitOfWork.Complete();

            return profile;
        }

        public async Task<EditProfileDto> GetMyProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found");

            var profile = await _unitOfWork.Repository<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FullName = user.FullName
                };
                await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
                await _unitOfWork.Complete();
            }

            return new EditProfileDto
            {
                FullName = profile.FullName,
                ImageUrl = profile.ImageUrl,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };
        }

        public async Task UpdateMyProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId)
                       ?? throw new KeyNotFoundException("User not found");

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            var profile = await _unitOfWork.Repository<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    FullName = dto.FullName
                };
                await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            }
            else
            {
                profile.FullName = dto.FullName;
            }

            await _userManager.UpdateAsync(user);
            await _unitOfWork.Complete();
        }

        public async Task UpdateProfileImageAsync(string userId, string imageUrl)
        {
            var profile = await _unitOfWork.Repository<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userId,
                    ImageUrl = imageUrl
                };
                await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            }
            else
            {
                profile.ImageUrl = imageUrl;
            }

            await _unitOfWork.Complete();
        }

        public async Task RemoveProfileImageAsync(string userId)
        {
            var profile = await _unitOfWork.Repository<UserProfile>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return;

            profile.ImageUrl = null;
            await _unitOfWork.Complete();
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
