using Microsoft.EntityFrameworkCore;
using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Common.Enums;

namespace TimesynqServer.Persistence.Repository
{
    public class UserRepository : IUserRepository
    {

        private readonly TimesynqDbContext _dbContext;

        public UserRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> UserExistsAsync(Guid userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .AnyAsync();
        }

        public async Task<MeProjection?> GetMeByIdAsync(Guid userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new MeProjection(
                    u.Id,
                    u.UserName!,
                    u.ProfilePicture,
                    u.CreatedOnUTC,
                    u.Followers.Count,
                    u.Followees.Count,
                    u.Email!,
                    u.EmailConfirmed
                ))
                .FirstOrDefaultAsync();
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
                    u.CreatedOnUTC,
                    u.Followers.Count,
                    u.Followees.Count
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<ProfileProjection?> GetProfileByIdAsync(Guid callerId, Guid userId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new ProfileProjection(
                    new UserProjection(
                        u.Id,
                        u.UserName!,
                        u.ProfilePicture,
                        u.CreatedOnUTC,
                        u.Followers.Count,
                        u.Followees.Count
                    ),
                    _dbContext.Follows.Any(f => f.FollowerId == callerId && f.FolloweeId == userId)
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
                    u.CreatedOnUTC,
                    u.Followers.Count,
                    u.Followees.Count
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

        public async Task<IEnumerable<UserProjection>> GetUsersContainingSearchStringAsync(string searchString, int pageNumber, int pageSize, SortOrder sortOrder, UserSortBy sortBy)
        {
            var query = _dbContext.Users
                .AsNoTracking()
                .Where(u => u.UserName!.StartsWith(searchString));

            query = (sortOrder, sortBy) switch
            {
                (SortOrder.Default, UserSortBy.UserName) => query.OrderBy(u => u.UserName),
                (SortOrder.Reverse,  UserSortBy.UserName) => query.OrderByDescending(u => u.UserName),

                (SortOrder.Default, UserSortBy.Followers) => query.OrderByDescending(u => u.Followers.Count),
                (SortOrder.Reverse, UserSortBy.Followers) => query.OrderBy(u => u.Followers.Count),

                (SortOrder.Default, UserSortBy.AccountAge) => query.OrderBy(u => u.CreatedOnUTC),
                (SortOrder.Reverse, UserSortBy.AccountAge) => query.OrderByDescending(u => u.CreatedOnUTC),

                _ => query.OrderBy(u => u.UserName)
            };

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserProjection
                (
                    u.Id,
                    u.UserName!,
                    u.ProfilePicture,
                    u.CreatedOnUTC,
                    u.Followers.Count,
                    u.Followees.Count
                ))
                .ToListAsync();
        }
    }
}
