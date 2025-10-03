using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Follows
{
    public interface IFollowRepository
    {
        public Task<FollowProjection?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<int> GetFollowersCountAsync(Guid followeeId);
        public Task<int> GetFolloweesCountAsync(Guid followerId);
        public Task<IEnumerable<UserProjection>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize, SortOrder sortOrder, FollowSortBy sortBy);
        public Task<IEnumerable<UserProjection>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize, SortOrder sortOrder, FollowSortBy sortBy);
        public Task AddFollowAsync(Follow follow);
        public Task<int> DeleteFollowAsync(Guid followerId, Guid followeeId);
    }
}
