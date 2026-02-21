using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text.Json;
using TimesynqServer.Common;
using TimesynqServer.Domain.Cache.Tracker;
using TimesynqServer.Infrastructure.Hubs.TrackerHub;

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
            public static string RoomKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{wipId}";
            public static string RoomConnectionsSetKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{wipId}:{TrackerHubCachePrefixes.Connections}";
        }

        private static class LuaScripts
        {
            public static readonly LuaScript RoomRemoveScript =
                LuaScript.Prepare(
                    File.ReadAllText(
                        Path.Combine(
                            AppContext.BaseDirectory, "Scripts/remove_connection_and_cleanup_if_empty.lua"
                        )
                    )
                );
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
        public async Task<TrackerConnection?> GetConnectionAsync(Guid userId, Guid wipId)
        {
            string key = CacheKeyBuilder.ConnectionKey(userId);

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            TrackerConnection? connection = JsonSerializer.Deserialize<TrackerConnection?>(stringResult!);
            if (connection == null || connection.WipId != wipId)
            {
                return null;
            }

            return connection;
        }

        /// <inheritdoc/>
        public async Task<bool> SetConnectionAsync(Guid userId, TrackerConnection connection)
        {
            string key = CacheKeyBuilder.ConnectionKey(userId);
            string roomKey = CacheKeyBuilder.RoomKey(connection.WipId);
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(connection.WipId);

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            string serializedConnection = JsonSerializer.Serialize(connection);

            _ = tran.StringSetAsync(key, serializedConnection);
            _ = tran.SetAddAsync(roomConnectionsSetKey, $"{connection.UserId}");
            _ = tran.KeyPersistAsync(roomKey);

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveConnectionAsync(Guid userId, Guid wipId)
        {
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(wipId);

            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();

            _ = tran.KeyDeleteAsync(CacheKeyBuilder.ConnectionKey(userId));
            _ = tran.SetRemoveAsync(roomConnectionsSetKey, $"{userId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        /// <inheritdoc/>
        public async Task<TrackerHubResult> RemoveConnectionAndCleanupIfEmptyAsync(Guid userId, Guid wipId)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId);
            string roomKey = CacheKeyBuilder.RoomKey(wipId);
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(wipId);

            IDatabase db = _redis.GetDatabase();

            int result = (int)await db.ScriptEvaluateAsync(
                LuaScripts.RoomRemoveScript,
                new
                {
                    connectionKey = (RedisKey)connectionKey,
                    roomKey = (RedisKey)roomKey,
                    roomConnectionsSetKey = (RedisKey)roomConnectionsSetKey,
                    userId = userId.ToString(),
                    ttlSeconds = TrackerHubConstants.SecondsBeforeRoomClose
                }
            );

            return result == 1 ?
                TrackerHubResult.Success() :
                TrackerHubResult.Failure(TrackerHubError.FailedToLeaveRoom);
        }

        /// <inheritdoc/>
        public async Task<TrackerHubResult<Room>> GetOrCreateRoomAsync(Guid ownerId, Guid wipId)
        {
            string key = CacheKeyBuilder.RoomKey(wipId);
            var newRoom = new Room
            {
                OwnerId = ownerId,
                WipId = wipId,
            };
            string serializedRoom = JsonSerializer.Serialize(newRoom);

            IDatabase db = _redis.GetDatabase();

            bool roomCached = await db.StringSetAsync(key, serializedRoom, when: When.NotExists);
            if (roomCached)
            {
                return newRoom;
            }

            string? stringResult = await db.StringGetAsync(key);

            if (string.IsNullOrEmpty(stringResult))
            {
                return TrackerHubResult<Room>.Failure(TrackerHubError.FailedToReadRoom);
            }

            Room? room = JsonSerializer.Deserialize<Room>(stringResult);
            if (room == null)
            {
                return TrackerHubResult<Room>.Failure(TrackerHubError.FailedToReadRoom);
            }

            return room;
        }

        /// <inheritdoc/>
        public async Task<Room?> GetRoomAsync(Guid wipId)
        {
            string key = CacheKeyBuilder.RoomKey(wipId);

            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize<Room>(stringResult!);
        }

        /// <inheritdoc/>
        public async Task<int> GetRoomUserCountAsync(Guid wipId)
        {
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(wipId);

            IDatabase db = _redis.GetDatabase();
            long length = await db.SetLengthAsync(roomConnectionsSetKey);
            return (int)length;
        }

        /// <inheritdoc/>
        public async Task<bool> SetRoomAsync(Room room)
        {
            string key = CacheKeyBuilder.RoomKey(room.WipId);

            IDatabase db = _redis.GetDatabase();
            string serializedRoom = JsonSerializer.Serialize(room);

            bool isWriteSuccessful = await db.StringSetAsync(key, serializedRoom);

            return isWriteSuccessful;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveRoomAsync(Guid wipId)
        {
            string roomKey = CacheKeyBuilder.RoomKey(wipId);
            string roomConnectionsSetKey = CacheKeyBuilder.RoomConnectionsSetKey(wipId);

            IDatabase db = _redis.GetDatabase();
            RedisValue[] connectionUserIds = await db.SetMembersAsync(roomConnectionsSetKey);

            //todo: remove tracker info related to room
            ITransaction tran = db.CreateTransaction();

            _ = tran.KeyExpireAsync(roomKey, TimeSpan.FromSeconds(TrackerHubConstants.SecondsBeforeRoomClose));
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
