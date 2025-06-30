using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache.HubCache
{
    public interface IHubCacheService : ICacheService
    {
        public Task<Room> GetRoomAsync(Guid ownerId, string roomCode);
        public Task<Room> GetRoomAsync(string roomCode);

        public Task<Connection> GetConnectionAsync(Guid userId);
        public Task<Connection> GetConnectionAsync(Guid userId, string roomCode);

        public Task<IEnumerable<Connection>> GetRoomConnectionsAsync(string roomCode);
    }
}
