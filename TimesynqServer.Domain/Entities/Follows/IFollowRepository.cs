using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Follows
{
    public interface IFollowRepository
    {
        public Task<FollowProjection?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<int> GetFollowersCountAsync(Guid followeeId, string? searchString);
        public Task<int> GetFolloweesCountAsync(Guid followerId, string? searchString);
        public Task<IEnumerable<UserProjection>> GetFollowersAsync(Guid followeeId, string? searchString, int pageNumber, int pageSize, SortOrder sortOrder, FollowSortBy sortBy);
        public Task<IEnumerable<UserProjection>> GetFolloweesAsync(Guid followerId, string? searchString, int pageNumber, int pageSize, SortOrder sortOrder, FollowSortBy sortBy);
        public Task AddFollowAsync(Follow follow);
        public Task<int> DeleteFollowAsync(Guid followerId, Guid followeeId);
    }
}
