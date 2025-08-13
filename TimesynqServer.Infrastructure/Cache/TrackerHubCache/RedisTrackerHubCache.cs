using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text.Json;
using TimesynqServer.Domain.Cache;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache
{
    /// <summary>
    /// Redis implementation for managing TrackerHub connections and rooms.
    /// </summary>
    public class RedisTrackerHubCache : ITrackerHubCache
    {

        private IConnectionMultiplexer _redis;

        public RedisTrackerHubCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <inheritdoc/>
        public async Task<Connection?> GetConnectionAsync(Guid userId)
        {
            string key = $"{TrackerHubCachePrefixes.Connection}:{userId}";

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize<Connection>(stringResult!);
        }

        /// <inheritdoc/>
        public async Task<Connection?> GetConnectionAsync(Guid userId, string roomCode)
        {
            string key = $"{TrackerHubCachePrefixes.Connection}:{userId}";

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            Connection? connection = JsonSerializer.Deserialize<Connection?>(stringResult!);
            if (connection == null || connection.RoomCode != roomCode)
            {
                return null;
            }

            return connection;
        }

        /// <inheritdoc/>
        public async Task<bool> SetConnectionAsync(Guid userId, Connection connection)
        {
            string key = $"{TrackerHubCachePrefixes.Connection}:{userId}";

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            string serializedConnection = JsonSerializer.Serialize(connection);

            _ = await tran.StringSetAsync(key, serializedConnection);
            _ = await tran.SetAddAsync($"{TrackerHubCachePrefixes.Room}:{connection.RoomCode}:{TrackerHubCachePrefixes.Connections}", $"{connection.UserId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveConnectionAsync(Guid userId, string roomCode)
        {
            string roomConnectionsSetKey = $"{TrackerHubCachePrefixes.Room}:{roomCode}:{TrackerHubCachePrefixes.Connections}";

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();

            _ = await tran.KeyDeleteAsync($"{TrackerHubCachePrefixes.Connection}:{userId}");
            _ = await tran.SetRemoveAsync(roomConnectionsSetKey, $"{userId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<Room?> GetRoomAsync(string roomCode)
        {
            string key = $"{TrackerHubCachePrefixes.Room}:{roomCode}";

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize<Room>(stringResult!);
        }

        /// <inheritdoc/>
        public async Task<Room?> GetRoomAsync(string roomCode, Guid ownerId)
        {
            string key = $"{TrackerHubCachePrefixes.Room}:{roomCode}";

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            Room? room = JsonSerializer.Deserialize<Room?>(stringResult!);
            if (room == null || room.OwnerId != ownerId)
            {
                return null;
            }

            return room;
        }

        /// <inheritdoc/>
        public async Task<bool> SetRoomAsync(string roomCode, Room room)
        {
            string key = $"{TrackerHubCachePrefixes.Room}:{roomCode}";

            IDatabase db = _redis.GetDatabase();
            string serializedRoom = JsonSerializer.Serialize(room);

            bool isWriteSuccessful = await db.StringSetAsync(key, serializedRoom);

            return isWriteSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveRoomAsync(string roomCode)
        {
            string roomConnectionsSetKey = $"{TrackerHubCachePrefixes.Room}:{roomCode}:{TrackerHubCachePrefixes.Connections}";

            //get connections to the room
            IDatabase db = _redis.GetDatabase();
            RedisValue[] connectionUserIds = await db.SetMembersAsync(roomConnectionsSetKey);

            //close the room and all connections
            //todo: remove tracker info related to room
            ITransaction tran = db.CreateTransaction();

            _ = await tran.KeyDeleteAsync($"{TrackerHubCachePrefixes.Room}:{roomCode}");
            _ = await tran.KeyDeleteAsync(roomConnectionsSetKey);
            foreach (RedisValue connectionUserId in connectionUserIds)
            {
                _ = await tran.KeyDeleteAsync($"{TrackerHubCachePrefixes.Connection}:{connectionUserId}");
            }

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }
    }
}
