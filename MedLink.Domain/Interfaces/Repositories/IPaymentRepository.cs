using MedLink.Domain.Entities.Payments;

namespace MedLink.Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByAppointmentIdAsync(int appointmentId);
        Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId);
        Task<List<Payment>> GetPaymentsByUserAsync(string userId);
        Task<Payment> AddAsync(Payment payment);
        Task<Payment> UpdateAsync(Payment payment);
        Task<bool> ExistsForAppointmentAsync(int appointmentId);
    }
}
