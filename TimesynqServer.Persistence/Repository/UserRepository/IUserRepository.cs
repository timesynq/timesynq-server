using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Persistence.Repository.UserRepository
{
    public interface IUserRepository
    {
        public Task<UserProjection?> GetByIdAsync(Guid userId);
        public Task<bool> GetConfirmedUserByIdAsync(Guid userId);
        public Task<int> GetTotalUsersContainingSearchStringAsync(string searchString);
        public Task<IEnumerable<UserProjection>> GetUsersContainingSearchStringAsync(string searchString, int pageNumber, int pageSize);
    }
}
