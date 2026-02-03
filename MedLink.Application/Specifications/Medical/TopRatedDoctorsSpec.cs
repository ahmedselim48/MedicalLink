using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Specifications.Medical
{
    public class TopRatedDoctorsSpec : BaseSpecification<Doctor>
    {
        public TopRatedDoctorsSpec(int count)
        {
            AddOrderByDesc(d => d.Reviews.Any() ? d.Reviews.Average(r => r.Rating) : 0);

            AddIncludes(d => d.Specialization);
            AddIncludes(d => d.Reviews);
            AddIncludes(d => d.Availabilities);

            ApplyPagination(0, count);
        }
    }
}
