using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IShareService
    {
        public Task<IEnumerable<SharedUserDTO>> GetSharedUsersAsync(Guid callerId, Guid wipId);
        public Task<PagedResult<SharedWipDTO>> GetSharedWipsAsync(Guid callerId, bool isAccepted, string? searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<Result<UserDTO>> ShareWipAsync(Guid callerId, Guid wipId, Guid shareWithId);
        public Task<Result> AcceptShareAsync(Guid callerId, Guid wipId);
        public Task<Result> UnshareFromWipAsync(Guid callerId, Guid wipId, Guid sharedWithId);
        public Task<Result> UnshareAllFromWipAsync(Guid callerId, Guid wipId);
    }
}
