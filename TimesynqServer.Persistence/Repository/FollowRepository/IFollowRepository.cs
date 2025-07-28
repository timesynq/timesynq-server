using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;
using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Database.Repository.FollowRepository
{
    public interface IFollowRepository
    {
        public Task<FollowProjection?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<int> GetFollowersCountAsync(Guid followeeId);
        public Task<int> GetFolloweesCountAsync(Guid followerId);
        public Task<IEnumerable<UserProjection>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize);
        public Task<IEnumerable<UserProjection>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize);
        public Task<FollowProjection> AddFollowAsync(Guid followerId, Guid followeeId);
        public Task DeleteFollowAsync(Guid followerId, Guid followeeId);
    }
}
