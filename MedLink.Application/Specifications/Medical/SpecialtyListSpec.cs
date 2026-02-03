using MedLink.Domain.Entities.Medical;

namespace MedLink.Application.Specifications.Medical
{
    public class SpecialtyListSpec : BaseSpecification<Specialization>
    {
        public SpecialtyListSpec(int? count = null)
        {
            AddOrderBy(s => s.Name);

            if (count.HasValue)
            {
                ApplyPagination(0, count.Value);
            }
        }
    }
}
