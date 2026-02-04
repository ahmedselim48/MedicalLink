using MedLink.Domain.Entities.Payments;

namespace MedLink.Application.Specifications.Payments
{
    /// <summary>
    /// Specification to get all payments for a specific user with pagination
    /// </summary>
    public class PaymentsByUserSpec : BaseSpecification<Payment>
    {
        public PaymentsByUserSpec(string userId)
            : base(p => p.Appointment.BookedByUserId == userId && !p.IsDeleted)
        {
            AddIncludes(p => p.Appointment);
            AddOrderByDesc(p => p.CreatedAt);
        }

        public PaymentsByUserSpec(string userId, int pageIndex, int pageSize)
            : base(p => p.Appointment.BookedByUserId == userId && !p.IsDeleted)
        {
            AddIncludes(p => p.Appointment);
            AddOrderByDesc(p => p.CreatedAt);
            ApplyPagination((pageIndex - 1) * pageSize, pageSize);
        }
    }
}
