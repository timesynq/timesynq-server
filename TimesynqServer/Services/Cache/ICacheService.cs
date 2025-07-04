using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache
{
    public interface ICacheService
    {
        public Task<T?> GetAsync<T>(string key) where T : class;
        public Task<bool> SetAsync<T>(string key, T value) where T : class;
        public Task<bool> RemoveAsync(string key);
    }
}
