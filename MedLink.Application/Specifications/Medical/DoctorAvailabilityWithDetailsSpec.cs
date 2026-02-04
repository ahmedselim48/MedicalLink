using MedLink.Domain.Entities.Appointments;

namespace MedLink.Application.Specifications.Medical
{
    public class DoctorAvailabilityWithDetailsSpec : BaseSpecification<DoctorAvailability>
    {
        public DoctorAvailabilityWithDetailsSpec(int id) : base(x => x.Id == id)
        {
            AddIncludes(x => x.Doctor);
        }
    }
}
