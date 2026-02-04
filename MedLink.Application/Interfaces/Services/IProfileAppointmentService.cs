using MedLink.Application.DTOs.UserProfile;

namespace MedLink.Application.Interfaces.Services
{
    public interface IProfileAppointmentService
    {
        Task<PagedResult<AppointmentListItemDto>> GetPastAsync(string userId, int page, int pageSize);

        Task<PagedResult<AppointmentListItemDto>> GetUpcomingAsync(string userId, int page, int pageSize);

        //Task CancelAsync(
        //    string userId,
        //    int appointmentId,
        //    string? reason
        //);
    }


}
