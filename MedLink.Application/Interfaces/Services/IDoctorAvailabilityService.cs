using MedLink.Application.DTOs.Medical;
using MedLink.Application.DTOs.Appointments;

namespace MedLink.Application.Interfaces.Services
{
    public interface IDoctorAvailabilityService
    {
        Task<DoctorAvailabilityDto> AddSingleSlotAsync(AddSlotRequest request, string? userId = null);
        Task<List<DoctorAvailabilityDto>> AddDayScheduleAsync(AddDayScheduleRequest request, string? userId = null);
        Task<List<DoctorAvailabilityDto>> AddWeekScheduleAsync(AddWeekScheduleRequest request, string? userId = null);
        
        Task DeleteSlotAsync(int slotId);
        Task<List<DoctorAvailabilityDto>> GetAllDoctorSlotsAsync(int doctorId);
        Task<List<DoctorAvailabilityDto>> GetAvailableSlotsAsync(int doctorId, DateTime? date = null);
        Task<int> GetDoctorIdByUserIdAsync(string userId);
    }
}
