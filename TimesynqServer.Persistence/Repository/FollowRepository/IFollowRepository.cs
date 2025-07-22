using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;

namespace TimesynqServer.Database.Repository.FollowRepository
{
    public interface IFollowRepository
    {
        public Task<Follow?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<int> GetFollowersCountAsync(Guid followeeId);
        public Task<int> GetFolloweesCountAsync(Guid followerId);
        public Task<IEnumerable<UserProjection>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize);
        public Task<IEnumerable<UserProjection>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize);
        public Task<Follow> FollowAsync(Guid followerId, Guid followeeId);
        public Task UnfollowAsync(Follow follow);
    }
}
