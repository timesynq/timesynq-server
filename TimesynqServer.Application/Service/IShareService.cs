using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IShareService
    {
        public Task<PagedResult<UserDTO>> GetSharedUsersAsync(Guid callerId, Guid wipId, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<PagedResult<WipDTO>> GetSharedWipsAsync(Guid callerId, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<Result<UserDTO>> ShareWipAsync(Guid callerId, Guid wipId, Guid userId);
        public Task<Result> UnshareFromWipAsync(Guid callerId, Guid wipId, Guid userId);
        public Task<Result> UnshareAllFromWipAsync(Guid callerId, Guid wipId);
    }
}
