using StackExchange.Redis;

namespace TimesynqServer.Services.Cache.RedisConnectionProvider
{
    public class RedisConnectionProvider : IRedisConnectionProvider
    {

        private readonly ConnectionMultiplexer _connection;

        public RedisConnectionProvider(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Redis") ?? "";
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public ConnectionMultiplexer GetConnection()
        {
            return _connection;
        }

    }
}
