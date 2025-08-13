using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service.FollowService
{
    public interface IFollowService
    {
        public Task<Result<FollowDTO>> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<PagedResult<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize, HttpRequest httpRequest);
        public Task<PagedResult<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize, HttpRequest httpRequest);
        public Task<Result<FollowDTO>> FollowAsync(Guid followerId, Guid followeeId);
        public Task<Result> UnfollowAsync(Guid followerId, Guid followeeId);
    }
}
