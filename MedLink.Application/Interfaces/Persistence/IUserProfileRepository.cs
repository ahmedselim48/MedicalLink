using MedLink.Domain.Entities.User;

namespace MedLink.Application.Interfaces.Persistence
{
    public interface IUserProfileRepository
    {
        Task<UserProfile?> GetByUserIdAsync(int userId);
        Task AddAsync(UserProfile profile);
        void Update(UserProfile profile);
    }
}
