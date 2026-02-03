using MedLink.Domain.Entities.Appointments;

namespace MedLink.Application.Specifications.Appointments
{
    public class AppointmentWithDetailsSpec : BaseSpecification<Appointment>
    {
        public AppointmentWithDetailsSpec(int id) : base(a => a.Id == id)
        {
            AddIncludes(a => a.Doctor);
            AddIncludes(a => a.Schedule);
            AddIncludes(a => a.Payment);
        }
    }
}
