using MedLink.Domain.Entities.User;

namespace MedLink.Application.Specifications.Users
{
    public class UserProfileSpec : BaseSpecification<UserProfile>
    {
        public UserProfileSpec(int pageIndex, int pageSize)
        {
            ApplyPagination(
                skip: (pageIndex - 1) * pageSize,
                take: pageSize
            );

            AddOrderBy(x => x.Id);
        }
    }
}
