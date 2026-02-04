namespace MedLink.Application.DTOs.UserProfile
{
    public class UserProfileDashboardDto
    {
        public ProfileHeaderDto Profile { get; set; } = null!;

        public int TotalAppointments { get; set; }
        public int UpcomingAppointments { get; set; }
        public int FavoriteDoctorsCount { get; set; }
    }
}
