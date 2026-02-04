using MedLink.Application.DTOs.Payments;

namespace MedLink.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request);
        Task<bool> ConfirmPaymentByStripeIdAsync(string stripeId, bool isSession, string? paymentIntentId = null);
        Task<bool> ConfirmPaymentByAppointmentIdAsync(int appointmentId);
        Task<PaymentDto> GetPaymentByAppointmentIdAsync(int appointmentId);
        Task<List<PaymentDto>> GetMyPaymentsAsync(string userId);

    }
}
