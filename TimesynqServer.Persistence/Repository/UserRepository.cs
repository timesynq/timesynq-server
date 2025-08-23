using Microsoft.EntityFrameworkCore;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Persistence.Repository
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

        public async Task<TimesynqUser?> GetTrackedUserByIdAsync(Guid userId)
        {
            return await _dbContext.Users
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> GetConfirmedUserByIdAsync(Guid userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.EmailConfirmed)
                .FirstOrDefaultAsync();
        }

        public async Task<UserProjection?> GetByUserNameAsync(string userName)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.NormalizedUserName == userName.ToUpper() || u.NormalizedSavedUserName == userName.ToUpper())
                .Select(u => new UserProjection(
                    u.Id,
                    u.UserName!,
                    u.ProfilePicture,
                    u.CreatedOnUTC
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetTotalUsersContainingSearchStringAsync(string searchString)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.UserName!.StartsWith(searchString))
                .CountAsync();
        }

        public async Task<IEnumerable<UserProjection>> GetUsersContainingSearchStringAsync(string searchString, int pageNumber, int pageSize)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.UserName!.StartsWith(searchString))
                .OrderBy(u => u.CreatedOnUTC)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserProjection
                (
                    u.Id,
                    u.UserName!,
                    u.ProfilePicture,
                    u.CreatedOnUTC
                ))
                .ToListAsync();
        }
    }
}
