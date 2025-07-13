using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.UserRepository
{
    public class UserRepository : IUserRepository
    {

        private readonly TimesynqDbContext _dbContext;

        public UserRepository(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TimesynqUser?> GetById(Guid userId)
        {
            TimesynqUser? user = await _dbContext.Users.FindAsync(userId);

            if (user == null)
            {
                return null;
            }

            return user;
        }
    }
}
