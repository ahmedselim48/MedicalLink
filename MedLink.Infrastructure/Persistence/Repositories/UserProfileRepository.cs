using MedLink.Application.Interfaces.Persistence;
using MedLink.Domain.Entities.User;
using MedLink.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedLink.Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfile?> GetByUserIdAsync(int userId)
        {
            return await _context.UserProfiles
                .FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task AddAsync(UserProfile profile)
        {
            await _context.UserProfiles.AddAsync(profile);
        }

        public void Update(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
        }
    }



}
