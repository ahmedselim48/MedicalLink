using MedLink.Domain.Entities.User;

namespace MedLink.Application.Specifications.Users
{
    public class UserFavoriteDoctorsSpec : BaseSpecification<Favorite>
    {
        public UserFavoriteDoctorsSpec(string userId)
            : base(f => f.UserId == userId)
        {
            AddIncludes(f => f.Doctor);
            AddIncludes(f => f.Doctor.Specialization);
            AddIncludes(f => f.Doctor.Reviews);
        }

        public UserFavoriteDoctorsSpec(string userId, int doctorId)
            : base(f => f.UserId == userId && f.DoctorId == doctorId)
        {
        }
    }
}
