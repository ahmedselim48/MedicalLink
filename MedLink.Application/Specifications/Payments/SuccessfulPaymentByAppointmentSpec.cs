using MedLink.Domain.Entities.Payments;
using MedLink.Domain.Enums;

namespace MedLink.Application.Specifications.Payments
{
    /// <summary>
    /// Specification to get successful payment for an appointment (for idempotency check)
    /// </summary>
    public class SuccessfulPaymentByAppointmentSpec : BaseSpecification<Payment>
    {
        public SuccessfulPaymentByAppointmentSpec(int appointmentId)
            : base(p => p.AppointmentId == appointmentId
                     && p.Status == PaymentStatus.Succeeded
                     && !p.IsDeleted)
        {
        }
    }
}
