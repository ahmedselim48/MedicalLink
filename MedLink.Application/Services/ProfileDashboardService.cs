using MedLink.Application.DTOs.UserProfile;
using MedLink.Application.Interfaces.Persistence;
using MedLink.Application.Interfaces.Services;
using MedLink.Application.Specifications.Appointments;
using MedLink.Application.Specifications.Users;
using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Entities.User;
using MedLink.Domain.Identity;
using Microsoft.AspNetCore.Identity;

public class ProfileDashboardService : IProfileDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;


    public ProfileDashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<UserProfileDashboardDto> GetMyDashboardAsync(string userId)
    {
        var profile = await _unitOfWork
            .Repository<UserProfile>()
            .FindAsync(x => x.UserId == userId);

        if (profile == null)
            throw new KeyNotFoundException("User profile not found");

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found");

        var totalAppointments = await _unitOfWork
            .Repository<Appointment>()
            .GetCountAsync(new AppointmentsByUserSpec(userId));

        var upcomingAppointments = await _unitOfWork
            .Repository<Appointment>()
            .GetCountAsync(new UpcomingAppointmentsSpec(userId));

        var favoriteDoctorsCount = await _unitOfWork
            .Repository<Favorite>()
            .GetCountAsync(new UserFavoriteDoctorsSpec(userId));

        return new UserProfileDashboardDto
        {
            Profile = new ProfileHeaderDto
            {
                FullName = profile.FullName,
                Email = user.Email ?? "",
                ImageUrl = profile.ImageUrl,
                MemberSince = profile.CreatedAt
            },
            TotalAppointments = totalAppointments,
            UpcomingAppointments = upcomingAppointments,
            FavoriteDoctorsCount = favoriteDoctorsCount
        };
    }


}
