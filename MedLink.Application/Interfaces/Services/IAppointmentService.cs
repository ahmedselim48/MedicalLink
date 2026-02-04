using MedLink.Application.DTOs.Appointments;
using MedLink.Application.DTOs.Medical;

namespace MedLink.Application.Interfaces.Services
{
    public interface IAppointmentService
    {
        Task<List<DoctorAvailabilityDto>> GetAvailableSlotsAsync(int doctorId, DateTime? date);
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentRequest request);
        Task<AppointmentDto> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto> UpdateAppointmentAsync(int id, UpdateAppointmentRequest request);
        Task<List<AppointmentDto>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date);
        Task<List<AppointmentDto>> GetMyAppointmentsAsync(string userId);
        Task<List<AppointmentDto>> GetUpcomingAppointmentsAsync(string userId);
        Task<AppointmentDto> RescheduleAppointmentAsync(int id, int newScheduleId, string userId);
        Task CancelAppointmentAsync(int id, string reason, string userId);
        Task CompleteAppointmentAsync(int id, string userId);
    }
}
