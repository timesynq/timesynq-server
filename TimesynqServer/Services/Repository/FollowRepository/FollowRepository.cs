using Microsoft.EntityFrameworkCore;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Migrations;
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
                .Where(f => f.FollowerId == followerId && f.FolloweeId == followeeId)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetFollowersCountAsync(Guid followeeId)
        {
            return await _dbContext.Follows
                .Where(f => f.FolloweeId == followeeId)
                .CountAsync();
        }

        public async Task<int> GetFolloweesCountAsync(Guid followerId)
        {
            return await _dbContext.Follows
                .Where(f => f.FollowerId == followerId)
                .CountAsync();
        }

        public async Task<IEnumerable<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .Where(f => f.FolloweeId == followeeId)
                .Include(f => f.Follower)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower!.ToUserDTO())
                .ToListAsync();
        }

        public async Task<IEnumerable<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize)
        {
            return await _dbContext.Follows
                .Where(f => f.FollowerId == followerId)
                .Include(f => f.Followee)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Followee!.ToUserDTO())
                .ToListAsync();
        }

        public async Task FollowAsync(Guid followerId, Guid followeeId)
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
            };
            await _dbContext.Follows.AddAsync(follow);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UnfollowAsync(Follow follow)
        {
            _dbContext.Remove(follow);
            await _dbContext.SaveChangesAsync();
        }

    }
}
