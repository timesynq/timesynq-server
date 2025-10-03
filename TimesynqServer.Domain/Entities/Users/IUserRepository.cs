using TimesynqServer.Common.Enums;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Domain.Entities.Users
{
    public interface IUserRepository
    {
        public Task<bool> UserExistsAsync(Guid userId);
        public Task<UserProjection?> GetByIdAsync(Guid userId);
        public Task<ProfileProjection?> GetProfileByIdAsync(Guid callerId, Guid userId);
        public Task<TimesynqUser?> GetTrackedUserByIdAsync(Guid userId);
        public Task<bool> GetConfirmedUserByIdAsync(Guid userId);
        public Task<UserProjection?> GetByUserNameAsync(string userName);
        public Task<int> GetTotalUsersContainingSearchStringAsync(string searchString);
        public Task<IEnumerable<UserProjection>> GetUsersContainingSearchStringAsync(string searchString, int pageNumber, int pageSize, SortOrder sortOrder, UserSortBy sortBy);
    }
}
