using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Repository.FollowRepository
{
    public interface IFollowRepository
    {
        public Task<Follow?> GetFollowAsync(Guid followerId, Guid followeeId);
        public Task<int> GetFollowersCountAsync(Guid followeeId);
        public Task<int> GetFolloweesCountAsync(Guid followerId);
        public Task<IEnumerable<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize);
        public Task<IEnumerable<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize);
        public Task<Follow> FollowAsync(Guid followerId, Guid followeeId);
        public Task UnfollowAsync(Follow follow);
    }
}
