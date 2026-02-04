using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;

namespace MedLink.Application.Specifications.Appointments
{
    public class UpcomingAppointmentsByUserSpec
     : BaseSpecification<Appointment>
    {
        public UpcomingAppointmentsByUserSpec(string userId)
            : base(a =>
                a.BookedByUserId == userId &&
                a.Status != AppointmentStatus.Cancelled &&
                (
                    a.Schedule.Date > DateTime.UtcNow.Date ||
                    (
                        a.Schedule.Date == DateTime.UtcNow.Date &&
                        a.Schedule.StartTime >= DateTime.UtcNow.TimeOfDay
                    )
                )
            )
        {
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Doctor.Specialization);
            AddIncludes(a => a.Schedule);

            AddOrderBy(a => a.Schedule.Date);
        }

        public UpcomingAppointmentsByUserSpec(
           string userId,
           int page,
           int pageSize)
           : this(userId)
        {
            ApplyPagination(
                (page - 1) * pageSize,
                pageSize
            );
        }
    }
}
