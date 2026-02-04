using MedLink.Domain.Entities.Appointments;

namespace MedLink.Application.Specifications.Appointments
{
    public class AppointmentsByDoctorSpec : BaseSpecification<Appointment>
    {
        public AppointmentsByDoctorSpec(int doctorId, DateTime? date)
            : base(a => a.DoctorId == doctorId)
        {
            AddIncludes(a => a.Schedule);
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Doctor.Specialization);

            if (date.HasValue)
            {
                Criteria = a => a.DoctorId == doctorId && a.Schedule.Date.Date == date.Value.Date;
            }

            AddOrderBy(a => a.Schedule.Date);
        }
    }
}
