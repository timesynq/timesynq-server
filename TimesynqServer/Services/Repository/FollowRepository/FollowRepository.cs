using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.FollowRepository
{
    public class FollowRepository : IFollowRepository
    {

        private readonly TimesynqDbContext _dbContext;

        public FollowRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Follow?> GetFollowAsync(Guid followerId, Guid followeeId)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId && f.FolloweeId == followeeId)
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

        public async Task<IEnumerable<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FolloweeId == followeeId)
                .OrderBy(f => f.CreatedOnUTC)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserDTO
                {
                    Id = f.Follower!.Id,
                    UserName = f.Follower.UserName!,
                    ProfilePicture = f.Follower.ProfilePicture!,
                    CreatedOnUTC = f.Follower.CreatedOnUTC,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .AsNoTracking()
                .Where(f => f.FollowerId == followerId)
                .OrderBy(f => f.CreatedOnUTC)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new UserDTO 
                {
                    Id = f.Followee!.Id,
                    UserName = f.Followee.UserName!,
                    ProfilePicture = f.Followee.ProfilePicture!,
                    CreatedOnUTC = f.Followee.CreatedOnUTC,
                })
                .ToListAsync();
        }

        public async Task<Follow> FollowAsync(Guid followerId, Guid followeeId)
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
            };
            await _dbContext.Follows.AddAsync(follow);
            await _dbContext.SaveChangesAsync();

            return follow;
        }

        public async Task UnfollowAsync(Follow follow)
        {
            _dbContext.Remove(follow);
            await _dbContext.SaveChangesAsync();
        }

    }
}
