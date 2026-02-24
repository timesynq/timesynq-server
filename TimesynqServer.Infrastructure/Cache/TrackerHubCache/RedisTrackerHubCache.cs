using StackExchange.Redis;
using TimesynqServer.Application.DTO;
using TimesynqServer.Common;
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
            public static string ConnectionKey(Guid userId, string connectionId) 
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Connection}:{userId}:{connectionId}";
            public static string RoomIndexKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{wipId}:index";
            public static string RoomConnectionsKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{wipId}:{TrackerHubCachePrefixes.Connections}";
            public static string RoomInfoKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCachePrefixes.Room}:{wipId}:info";
        }

        private static class LuaScripts
        {
            public static readonly LuaScript RoomJoinScript =
                LuaScript.Prepare(
                    File.ReadAllText(
                        Path.Combine(
                            AppContext.BaseDirectory, "Scripts/set_connection_and_create_room_if_empty.lua"
                        )
                    )
                );

            public static readonly LuaScript RoomLeaveScript =
                LuaScript.Prepare(
                    File.ReadAllText(
                        Path.Combine(
                            AppContext.BaseDirectory, "Scripts/remove_connection_and_cleanup_if_empty.lua"
                        )
                    )
                );

            public static readonly LuaScript RoomRemoveScript =
                LuaScript.Prepare(
                    File.ReadAllText(
                        Path.Combine(
                            AppContext.BaseDirectory, "Scripts/remove_room.lua"
                        )
                    )
                );
        }

        /// <inheritdoc/>
        public async Task<bool> SetConnectionAndCreateRoomIfEmptyAsync(Guid userId, TrackerConnection connection, WipDTO wipDTO)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connection.ConnectionId);
            string roomIndexKey = CacheKeyBuilder.RoomIndexKey(connection.WipId);
            string RoomConnectionsKey = CacheKeyBuilder.RoomConnectionsKey(connection.WipId);
            string roomInfoKey = CacheKeyBuilder.RoomInfoKey(connection.WipId);

            IDatabase db = _redis.GetDatabase();

            int result = (int)await db.ScriptEvaluateAsync(
                LuaScripts.RoomJoinScript,
                new
                {
                    connectionKey = (RedisKey)connectionKey,
                    roomIndexKey = (RedisKey)roomIndexKey,
                    RoomConnectionsKey = (RedisKey)RoomConnectionsKey,
                    roomInfoKey = (RedisKey)roomInfoKey,
                    wipId = connection.WipId,
                    connectionId = connection.ConnectionId,
                    wipNameFieldName = "wipId",
                    wipName = wipDTO.Name,
                    ownerIdFieldName = "ownerId",
                    ownerId = wipDTO.OwnerId.ToString()
                }
            );

            return result == 0;
        }

        /// <inheritdoc/>
        public async Task<TrackerConnection?> RemoveConnectionAndCleanupIfEmptyAsync(Guid userId, string connectionId)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            IDatabase db = _redis.GetDatabase();

            string? result = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.RoomLeaveScript,
                new
                {
                    connectionKey = (RedisKey)connectionKey,
                    ttlSeconds = TrackerHubConstants.SecondsBeforeRoomClose
                }
            );

            if (result == null)
                return null;

            return new TrackerConnection
            {
                UserId = userId,
                ConnectionId = connectionId,
                WipId = Guid.Parse(result)
            };
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveRoomAsync(Guid wipId)
        {
            string roomIndexKey = CacheKeyBuilder.RoomIndexKey(wipId);
            string roomConnectionsKey = CacheKeyBuilder.RoomConnectionsKey(wipId);

            IDatabase db = _redis.GetDatabase();
            int result = (int)await db.ScriptEvaluateAsync(
                LuaScripts.RoomJoinScript,
                new
                {
                    roomIndexKey = (RedisKey)roomIndexKey,
                    roomConnectionsKey = (RedisKey)roomConnectionsKey,
                }
            );

            return result == 0;
        }
    }
}
