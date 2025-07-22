using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;

namespace TimesynqServer.Database.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {

        private readonly TimesynqDbContext _dbContext;

        public UserRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserProjection?> GetByIdAsync(Guid userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserProjection
                (
                    u.Id,
                    u.UserName!,
                    u.ProfilePicture,
                    u.CreatedOnUTC
                ))
                .FirstOrDefaultAsync();
        }
    }
}
