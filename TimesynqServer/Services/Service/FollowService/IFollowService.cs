using TimesynqServer.Models.DTO;
using TimesynqServer.Models.Pagination;

namespace TimesynqServer.Services.Service.FollowService
{
    public interface IFollowService
    {
        public Task<FollowDTO?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<PagedResult<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize, HttpRequest httpRequest);
        public Task<PagedResult<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize, HttpRequest httpRequest);
        public Task<FollowDTO?> FollowAsync(Guid followerId, Guid followeeId);
        public Task UnfollowAsync(Guid followerId, Guid followeeId);
    }
}
