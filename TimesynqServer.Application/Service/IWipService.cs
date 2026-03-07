using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IWipService
    {
        public Task<WipDTO?> GetWipAsync(Guid callerId, Guid wipId);
        public Task<PagedResult<WipDTO>> GetMyWipsAsync(Guid callerId, string? searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<Result<WipDTO>> CreateWipAsync(Guid callerId);
        public Task<Result<WipDTO>> ChangeWipName(Guid callerId, Guid wipId, string newName);
        public Task<Result> DeleteWipAsync(Guid callerId, Guid wipId);
    }
}
