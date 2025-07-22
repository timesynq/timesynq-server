using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;
using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Database.Repository.FollowRepository
{
    public class FollowRepository : IFollowRepository
    {

        private readonly TimesynqDbContext _dbContext;

        public FollowRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<FollowProjection?> GetFollowAsync(Guid followerId, Guid followeeId)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId && f.FolloweeId == followeeId)
                .Select(f => new FollowProjection
                (
                    f.FollowerId,
                    f.FolloweeId,
                    f.CreatedOnUTC
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetFollowersCountAsync(Guid followeeId)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId)
                .CountAsync();
        }

        public async Task<int> GetFolloweesCountAsync(Guid followerId)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId)
                .CountAsync();
        }

        public async Task<IEnumerable<UserProjection>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId)
                .OrderBy(f => f.CreatedOnUTC)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserProjection
                (
                    f.Follower!.Id,
                    f.Follower.UserName!,
                    f.Follower.ProfilePicture!,
                    f.Follower.CreatedOnUTC
                ))
                .ToListAsync();
        }

        public async Task<IEnumerable<UserProjection>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId)
                .OrderBy(f => f.CreatedOnUTC)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserProjection
                (
                    f.Followee!.Id,
                    f.Followee.UserName!,
                    f.Followee.ProfilePicture!,
                    f.Followee.CreatedOnUTC
                ))
                .ToListAsync();
        }

        public async Task<FollowProjection> FollowAsync(Guid followerId, Guid followeeId)
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
            };
            await _dbContext.Follows.AddAsync(follow);
            await _dbContext.SaveChangesAsync();

            return new FollowProjection(follow);
        }

        public async Task UnfollowAsync(Guid followerId, Guid followeeId)
        {
            await _dbContext.Follows
                .Where(f => f.FollowerId == followerId && f.FolloweeId == followeeId)
                .ExecuteDeleteAsync(); //executes immediately, don't need to call SaveChangesAsync()
        }

    }
}
