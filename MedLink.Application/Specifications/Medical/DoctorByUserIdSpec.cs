using MedLink.Application.Specifications;
using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Specifications.Medical
{
    public class DoctorByUserIdSpec : BaseSpecification<Doctor>
    {
        public DoctorByUserIdSpec(string userId)
            : base(x => x.UserId == userId)
        {
        }
    }
}
