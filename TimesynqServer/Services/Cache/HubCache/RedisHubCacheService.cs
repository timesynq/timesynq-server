using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache.HubCache
{
    public class RedisHubCacheService : IHubCacheService
    {
        public Task<T?> GetAsync<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }

        public Task SetAsync<T>(string key, T value) where T : class
        {
            throw new NotImplementedException();
        }
        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByPrefix(string prefixKey)
        {
            throw new NotImplementedException();
        }

        public Task<Connection> GetConnectionAsync(string key, string roomCode)
        {
            throw new NotImplementedException();
        }

        public Task<Room> GetRoomAsync(string key, Guid ownerId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Connection>> GetRoomConnectionsAsync(string roomCode)
        {
            throw new NotImplementedException();
        }
        
    }
}
