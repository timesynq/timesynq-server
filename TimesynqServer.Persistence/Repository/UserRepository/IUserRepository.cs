using TimesynqServer.Database.Projections;

namespace TimesynqServer.Database.Repository.UserRepository
{
    public interface IUserRepository
    {
        public Task<UserProjection?> GetByIdAsync(Guid userId);
    }
}
