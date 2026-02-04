using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;

namespace MedLink.Application.Specifications.Appointments
{
    public class UpcomingAppointmentsSpec : BaseSpecification<Appointment>
    {
        public UpcomingAppointmentsSpec(string userId)
            : base(a => a.BookedByUserId == userId
                     && a.Schedule.Date >= DateTime.Today
                     && a.Status != AppointmentStatus.Cancelled
                     && a.Status != AppointmentStatus.Completed)
        {
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Doctor.Specialization);
            AddIncludes(a => a.Schedule);
            AddOrderBy(a => a.Schedule.Date);
        }
    }
}
