using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key) where T : class;
        public Task SetAsync<T>(string key, T value) where T : class;
        public Task RemoveAsync(string key);
        public Task RemoveByPrefix(string prefixKey);
    }
}
