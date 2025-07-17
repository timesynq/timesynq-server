using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.UserRepository
{
    public interface IUserRepository
    {
        public Task<TimesynqUser?> GetByIdAsync(Guid userId);
    }
}
