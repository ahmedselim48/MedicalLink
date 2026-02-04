using MedLink.Domain.Entities.Appointments;
using MedLink.Domain.Enums;

namespace MedLink.Application.Specifications.Appointments
{
    public class PastAppointmentsByUserSpec
        : BaseSpecification<Appointment>
    {
        public PastAppointmentsByUserSpec(string userId)
            : base(a =>
                a.BookedByUserId == userId &&
                a.Status != AppointmentStatus.Cancelled &&
                (
                    a.Schedule.Date < DateTime.UtcNow.Date ||
                    (
                        a.Schedule.Date == DateTime.UtcNow.Date &&
                        a.Schedule.StartTime < DateTime.UtcNow.TimeOfDay
                    )
                )
            )
        {
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Doctor.Specialization);
            AddIncludes(a => a.Schedule);

            AddOrderByDesc(a => a.Schedule.Date);
        }

        public PastAppointmentsByUserSpec(
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
