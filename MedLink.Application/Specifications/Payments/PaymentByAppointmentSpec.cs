using MedLink.Domain.Entities.Payments;

namespace MedLink.Application.Specifications.Payments
{
    /// <summary>
    /// Specification to get a payment by its associated appointment ID
    /// </summary>
    public class PaymentByAppointmentSpec : BaseSpecification<Payment>
    {
        public PaymentByAppointmentSpec(int appointmentId)
            : base(p => p.AppointmentId == appointmentId && !p.IsDeleted)
        {
            AddIncludes(p => p.Appointment);
        }
    }
}
