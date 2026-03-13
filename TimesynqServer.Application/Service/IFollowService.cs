using Microsoft.AspNetCore.Http;
using System.Globalization;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IFollowService
    {
        public Task<Result<FollowDTO>> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<PagedResult<UserDTO>> GetFollowersAsync(Guid followeeId, string? searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<PagedResult<UserDTO>> GetFolloweesAsync(Guid followerId, string? searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<Result<FollowDTO>> FollowAsync(Guid followerId, Guid followeeId);
        public Task<Result> UnfollowAsync(Guid followerId, Guid followeeId);
    }
}
