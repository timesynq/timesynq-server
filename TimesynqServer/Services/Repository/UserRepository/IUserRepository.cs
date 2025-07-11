using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.UserRepository
{
    public interface IUserRepository
    {
        public Task<UserDTO?> GetById(Guid userId);
    }
}
