
using MedLink.Domain.Entities.Appointments;

namespace MedLink.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment?> GetByIdWithDetailsAsync(int id);
        Task<List<Appointment>> GetAppointmentsByDoctorAsync(int doctorId, DateTime? date = null);
        Task<List<Appointment>> GetAppointmentsByUserAsync(string userId);
        Task<Appointment> AddAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task DeleteAsync(Appointment appointment);
        Task<bool> ExistsAsync(int id);

    }
}
