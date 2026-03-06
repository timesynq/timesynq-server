using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;
using TimesynqServer.Application.DTO;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
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
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Connection}:{userId}:{connectionId}";
            public static string RoomIndexKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Index}";
            public static string RoomConnectionsKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Connections}";
            public static string RoomInfoKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Info}";
        }

        private static class LuaScripts
        {
            private static string LoadEmbeddedScript(string filename)
            {
                const string ScriptPathPrefix = "TimesynqServer.Infrastructure.Cache.TrackerHubCache.Scripts";
                string path = $"{ScriptPathPrefix}.{filename}";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream? stream = assembly.GetManifestResourceStream(path) 
                    ?? throw new Exception($"Resource not found: {path}");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }

            public static readonly string RoomJoinScript =
                    LoadEmbeddedScript("set_connection_and_create_room_if_empty.lua");

            public static readonly string RoomLeaveScript =
                    LoadEmbeddedScript("remove_connection_and_cleanup_if_empty.lua");

            public static readonly string RoomRemoveScript =
                    LoadEmbeddedScript("remove_room.lua");
        }

        /// <inheritdoc/>
        public async Task<Guid?> GetRoomCodeAsync(Guid userId, string connectionId)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            IDatabase db = _redis.GetDatabase();

            string? roomCode = await db.HashGetAsync(connectionKey, "WipId");
            if (roomCode == null || !Guid.TryParse(roomCode, out Guid wipId))
            {
                return null;
            }

            return wipId;
        }

        /// <inheritdoc/>
        public async Task<TrackerHubResult<IEnumerable<RoomMember>>> SetConnectionAndCreateRoomIfEmptyAsync(UserDTO userDTO, TrackerConnection connection, WipDTO wipDTO)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userDTO.Id, connection.ConnectionId);
            string roomIndexKey = CacheKeyBuilder.RoomIndexKey(connection.WipId);
            string RoomConnectionsKey = CacheKeyBuilder.RoomConnectionsKey(connection.WipId);
            string roomInfoKey = CacheKeyBuilder.RoomInfoKey(connection.WipId);

            string payload = JsonSerializer.Serialize(new
            {
                WipId = connection.WipId.ToString(),
                ConnectionId = connection.ConnectionId,
                UserId = userDTO.Id.ToString(),
                UserName = userDTO.UserName,
                WipName = wipDTO.Name,
                OwnerId = wipDTO.OwnerId.ToString(),
            });

            IDatabase db = _redis.GetDatabase();

            string? jsonResult = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.RoomJoinScript,
                [
                    connectionKey,
                    roomIndexKey,
                    RoomConnectionsKey,
                    roomInfoKey,
                ], 
                [
                    payload
                ]
            );

            if (jsonResult == null)
            {
                return TrackerHubResult<IEnumerable<RoomMember>>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            IEnumerable<RoomMember>? roomMembers = JsonSerializer.Deserialize<IEnumerable<RoomMember>>(jsonResult);
            if (roomMembers == null)
            {
                return TrackerHubResult<IEnumerable<RoomMember>>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            return TrackerHubResult<IEnumerable<RoomMember>>.Success(roomMembers);
        }

        /// <inheritdoc/>
        public async Task<TrackerConnection?> RemoveConnectionAndCleanupIfEmptyAsync(Guid userId, string connectionId)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            IDatabase db = _redis.GetDatabase();

            string? result = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.RoomLeaveScript,
                [
                    connectionKey,
                ],
                [
                    TrackerHubConstants.SecondsBeforeRoomClose
                ]
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
                [
                    roomIndexKey,
                    roomConnectionsKey,
                ]
            );

            return result == 0;
        }
    }
}
