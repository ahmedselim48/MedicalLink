using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;

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

        public async Task<EditProfileDto> GetMyProfileAsync(string userId)
        {
            var profile = await _unitOfWork
                .Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

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
            // ===== UserProfile =====
            var profile = await _unitOfWork
                .Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            profile.FullName = dto.FullName;
            _unitOfWork.Repository<UserProfile>().Update(profile);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            user.PhoneNumber = dto.PhoneNumber;
            await _userManager.UpdateAsync(user);

            await _unitOfWork.Complete();
        }


        public async Task UpdateProfileImageAsync(string userId, string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("ImageUrl is required");

            var profile = await _unitOfWork
                .Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            profile.ImageUrl = imageUrl;

            _unitOfWork.Repository<UserProfile>().Update(profile);
            await _unitOfWork.Complete();
        }
        public async Task RemoveProfileImageAsync(string userId)
        {
            var profile = await _unitOfWork
                .Repository<UserProfile>()
                .FindAsync(x => x.UserId == userId);

            if (profile == null)
                throw new Exception("User profile not found");

            profile.ImageUrl = null;

            _unitOfWork.Repository<UserProfile>().Update(profile);
            await _unitOfWork.Complete();
        }

        public async Task CreateAsync(string userId, string fullName)
        {

            var profile = new UserProfile
            {
                UserId = userId,
                FullName = fullName
            };

            await _unitOfWork.Repository<UserProfile>().AddAsync(profile);
            await _unitOfWork.Complete();
        }
    }
}
