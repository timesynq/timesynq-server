using StackExchange.Redis;

namespace TimesynqServer.Services.Cache.RedisConnectionProvider
{
    public interface IRedisConnectionProvider
    {
        ConnectionMultiplexer GetConnection();
    }
}
