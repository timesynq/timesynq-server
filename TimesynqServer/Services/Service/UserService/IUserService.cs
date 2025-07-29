using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Service.UserService
{
    public interface IUserService
    {
        public Task<UserDTO?> GetUserAsync(Guid userId);
    }
}
