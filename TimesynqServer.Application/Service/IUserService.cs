using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IUserService
    {
        public Task<Result<MeDTO>> GetMeAsync(Guid userId);
        public Task<UserDTO?> GetUserAsync(Guid userId);
        public Task<ProfileDTO?> GetProfileAsync(Guid callerId, Guid userId);
        public Task<bool> IsUserConfirmed(Guid userId);
        public Task<PagedResult<UserDTO>> SearchUsers(string searchString, int pageNumber, int pageSize, string sortOrder, string sortBy, HttpRequest httpRequest);
        public Task<Result<UserDTO>> ChangeUserName(Guid userId, string newUserName);
        public Task<Result> DeleteAccount(Guid userId);
    }
}
