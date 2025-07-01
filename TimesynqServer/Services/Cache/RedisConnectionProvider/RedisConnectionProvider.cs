using StackExchange.Redis;

namespace TimesynqServer.Services.Cache.RedisConnectionProvider
{
    public class RedisConnectionProvider : IRedisConnectionProvider, IDisposable
    {

        private readonly Lazy<ConnectionMultiplexer> _connection;
        private bool _disposed;

        public RedisConnectionProvider(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("Redis") ?? "";
            _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(connectionString));
        }

        public ConnectionMultiplexer GetConnection()
        {
            return _connection.Value;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_connection.IsValueCreated)
            {
                _connection.Value.Dispose();
                _disposed = true;
            }
        }

    }
}
