using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache.HubCache
{
    public interface IHubCacheService : ICacheService
    {
        public Task<Room?> GetRoomAsync(string key, Guid ownerId);
        public Task<Connection?> GetConnectionAsync(string key, string roomCode);
        public Task<bool> SetConnectionAsync(string key, Connection connection);
        public Task<bool> CloseRoom(string roomCode);
    }
}
