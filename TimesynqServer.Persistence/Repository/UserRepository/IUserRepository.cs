using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Persistence.Repository.UserRepository
{
    public interface IUserRepository
    {
        public Task<UserProjection?> GetByIdAsync(Guid userId);
        public Task<bool> GetConfirmedUserByIdAsync(Guid userId);
    }
}
