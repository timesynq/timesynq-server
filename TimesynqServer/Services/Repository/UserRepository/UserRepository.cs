using Microsoft.AspNetCore.Identity;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {

        private readonly UserManager<TimesynqUser> _userManager;

        public UserRepository(UserManager<TimesynqUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<TimesynqUser?> GetByIdAsync(Guid userId)
        {
            return await _userManager.FindByIdAsync(userId.ToString());
        }
    }
}
