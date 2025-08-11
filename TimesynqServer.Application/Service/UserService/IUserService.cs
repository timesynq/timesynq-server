using TimesynqServer.Application.DTO;

namespace TimesynqServer.Application.Service.UserService
{
    public interface IUserService
    {
        public Task<UserDTO?> GetUserAsync(Guid userId);
    }
}
