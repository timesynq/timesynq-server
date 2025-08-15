using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;

namespace TimesynqServer.Application.Service.UserService
{
    public interface IUserService
    {
        public Task<UserDTO?> GetUserAsync(Guid userId);
        public Task<bool> IsUserConfirmed(Guid userId);
        public Task<PagedResult<UserDTO>> SearchUsers(string searchString, int pageNumber, int pageSize, HttpRequest httpRequest);
    }
}
