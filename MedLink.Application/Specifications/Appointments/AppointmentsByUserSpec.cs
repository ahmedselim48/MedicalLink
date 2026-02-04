using MedLink.Domain.Entities.Appointments;

namespace MedLink.Application.Specifications.Appointments
{
    public class AppointmentsByUserSpec : BaseSpecification<Appointment>
    {
        public AppointmentsByUserSpec(string userId)
            : base(a => a.BookedByUserId == userId)
        {
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Doctor.Specialization);
            AddIncludes(a => a.Schedule);
            AddOrderByDesc(a => a.CreatedAt);
        }
    }
}
