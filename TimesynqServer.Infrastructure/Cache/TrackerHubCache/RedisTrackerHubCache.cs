using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text.Json;
using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache
{
    /// <summary>
    /// Redis implementation for managing TrackerHub connections and rooms.
    /// </summary>
    public class RedisTrackerHubCache : ITrackerHubCache
    {

        private readonly IConnectionMultiplexer _redis;

        public RedisTrackerHubCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private static class CacheKeyBuilder
        {
            public static string ConnectionKey(Guid userId) 
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Connection}:{userId}";
            public static string RoomKey(string roomCode)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{roomCode}";
            public static string RoomConnectionsSetKey(string roomCode)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{roomCode}:{TrackerHubCachePrefixes.Connections}";
        }

        /// <inheritdoc/>
        public async Task<TrackerConnection?> GetConnectionAsync(Guid userId)
        {
            string key = CacheKeyBuilder.ConnectionKey(userId);

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize<TrackerConnection>(stringResult!);
        }

        /// <inheritdoc/>
        public async Task<TrackerConnection?> GetConnectionAsync(Guid userId, string roomCode)
        {
            string key = CacheKeyBuilder.ConnectionKey(userId);

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            TrackerConnection? connection = JsonSerializer.Deserialize<TrackerConnection?>(stringResult!);
            if (connection == null || connection.RoomCode != roomCode)
            {
                return null;
            }

            return connection;
        }

        /// <inheritdoc/>
        public async Task<bool> SetConnectionAsync(Guid userId, TrackerConnection connection)
        {
            string key = CacheKeyBuilder.ConnectionKey(userId);
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(connection.RoomCode);

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            string serializedConnection = JsonSerializer.Serialize(connection);

            _ = tran.StringSetAsync(key, serializedConnection);
            _ = tran.SetAddAsync(roomConnectionsSetKey, $"{connection.UserId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveConnectionAsync(Guid userId, string roomCode)
        {
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(roomCode);

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();

            _ = tran.KeyDeleteAsync(CacheKeyBuilder.ConnectionKey(userId));
            _ = tran.SetRemoveAsync(roomConnectionsSetKey, $"{userId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<Room?> GetRoomAsync(string roomCode)
        {
            string key = CacheKeyBuilder.RoomKey(roomCode);

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
            string key = CacheKeyBuilder.RoomKey(roomCode);

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
            string key = CacheKeyBuilder.RoomKey(roomCode);

            IDatabase db = _redis.GetDatabase();
            string serializedRoom = JsonSerializer.Serialize(room);

            bool isWriteSuccessful = await db.StringSetAsync(key, serializedRoom);

            return isWriteSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveRoomAsync(string roomCode)
        {
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(roomCode);

            IDatabase db = _redis.GetDatabase();
            RedisValue[] connectionUserIds = await db.SetMembersAsync(roomConnectionsSetKey);

            //todo: remove tracker info related to room
            ITransaction tran = db.CreateTransaction();

            _ = tran.KeyDeleteAsync(CacheKeyBuilder.RoomKey(roomCode));
            _ = tran.KeyDeleteAsync(roomConnectionsSetKey);
            foreach (RedisValue connectionUserId in connectionUserIds)
            {
                Guid userId = Guid.Parse(connectionUserId.ToString());
                _ = tran.KeyDeleteAsync(CacheKeyBuilder.ConnectionKey(userId));
            }

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }
    }
}
