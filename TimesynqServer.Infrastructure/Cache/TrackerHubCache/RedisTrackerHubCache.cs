using Amazon.SimpleEmail.Model;
using StackExchange.Redis;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Text.RegularExpressions;
using TimesynqServer.Application.DTO;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.TrackerCommandDTO;
using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache
{
    /// <summary>
    /// Redis implementation for managing TrackerHub connections and rooms.
    /// </summary>
    public class RedisTrackerHubCache : ITrackerHubCache
    {
        private const string NullColumn = "--";

        private readonly IConnectionMultiplexer _redis;

        public RedisTrackerHubCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private static class CacheKeyBuilder
        {
            public static string ConnectionKeyPrefix(Guid userId)
                 => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Connection}:{userId}";
            public static string ConnectionKey(Guid userId, string connectionId)
                => $"{ConnectionKeyPrefix(userId)}:{connectionId}";
            public static string ExtractConnectionIdFromConnectionKey(string connectionKey)
            {
                if (!Regex.IsMatch(connectionKey, @"^tracker:connection:[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}:[^\s]+$"))
                    throw new ArgumentException(
                        $"Argument ({connectionKey}) is not formatted properly." +
                        $"Use CacheKeyBuilder.ConnectionKey(Guid, string) to build a proper connection key."
                    );
                return connectionKey.Split(':')[^1];
            }

            public static string RoomIndexKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Index}";
            public static string RoomConnectionsKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Connections}";
            public static string RoomInfoKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Info}";
            public static string RoomLogKey(Guid wipId)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Log}";
            public static string FrameKey(Guid wipId, int frame)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Frame}:{Hex.TwoDigit(frame)}";
            public static string ChannelKey(Guid wipId, int frame, int channel)
                => $"{CachePrefixes.Tracker}:{TrackerHubCacheKeySegments.Room}:{wipId}:{TrackerHubCacheKeySegments.Frame}:{Hex.TwoDigit(frame)}:{TrackerHubCacheKeySegments.Channel}:{Hex.TwoDigit(channel)}";
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
            public static readonly string BpmUpdateScript =
                LoadEmbeddedScript("update_bpm.lua");
            public static readonly string LineCountUpdateScript =
                LoadEmbeddedScript("update_line_count.lua");
            public static readonly string LineUpdateScript =
                LoadEmbeddedScript("update_line.lua");
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
        public async Task<IEnumerable<string>> GetConnectionIdsAsync(Guid wipId, Guid userId)
        {
            string roomConnectionsKey = CacheKeyBuilder.RoomConnectionsKey(wipId);

            IDatabase db = _redis.GetDatabase();

            RedisValue[] connectionKeys = await db.SetMembersAsync(roomConnectionsKey);
            string userSpecificPrefix = CacheKeyBuilder.ConnectionKeyPrefix(userId);

            IEnumerable<RedisValue> connectionKeysBelongingToUser = connectionKeys.Where(
                key =>
                key.ToString().StartsWith(userSpecificPrefix)
            );

            return connectionKeysBelongingToUser.Select(
                key =>
                CacheKeyBuilder.ExtractConnectionIdFromConnectionKey(key.ToString())
            );
        }

        /// <inheritdoc/>
        public async Task<TrackerHubResult<IEnumerable<RoomMember>>> SetConnectionAndCreateRoomIfEmptyAsync(UserDTO userDTO, TrackerConnection connection, WipDTO wipDTO)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userDTO.Id, connection.ConnectionId);
            string roomIndexKey = CacheKeyBuilder.RoomIndexKey(connection.WipId);
            string roomConnectionsKey = CacheKeyBuilder.RoomConnectionsKey(connection.WipId);
            string roomInfoKey = CacheKeyBuilder.RoomInfoKey(connection.WipId);

            string payload = JsonSerializer.Serialize(new
            {
                WipId = connection.WipId.ToString(),
                ConnectionId = connection.ConnectionId,
                UserId = userDTO.Id.ToString(),
                UserName = userDTO.UserName,
                WipName = wipDTO.Name,
                OwnerId = wipDTO.OwnerId.ToString(),
                WipBpm = TrackerConstants.DefaultBpm, // placeholder; wipDTO should not contain bpm, so leave this until wipDTO param is replaced with the full tracker
            });

            IDatabase db = _redis.GetDatabase();

            string? jsonResult = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.RoomJoinScript,
                [
                    connectionKey,
                    roomIndexKey,
                    roomConnectionsKey,
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

        /// <inheritdoc/>
        public async Task<bool> ChangeWipNameAsync(Guid wipId, string newName)
        {
            string roomInfoKey = CacheKeyBuilder.RoomInfoKey(wipId);

            IDatabase db = _redis.GetDatabase();

            ITransaction tran = db.CreateTransaction();

            tran.AddCondition(Condition.KeyExists(roomInfoKey));
            Task<bool> setTask = tran.HashSetAsync(roomInfoKey, "WipName", newName);

            bool committed = await tran.ExecuteAsync();
            return committed && await setTask;
        }

        /// <inheritdoc/>
        public async Task<Guid?> UpdateBpmAsync(Guid userId, string connectionId, int newBpm)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            string payload = JsonSerializer.Serialize(new
            {
                UserID = userId.ToString(),
                NewBpm = newBpm.ToString(),
                UpdatedOnUTC = DateTime.UtcNow.ToString()
            });

            IDatabase db = _redis.GetDatabase();

            string? result = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.BpmUpdateScript,
                [
                    connectionKey,
                ],
                [
                    payload
                ]
            );

            if (result == null || !Guid.TryParse(result, out Guid wipId))
            {
                return null;
            }

            return wipId;
        }

        /// <inheritdoc/>
        public async Task<Guid?> UpdateLineCountAsync(Guid userId, string connectionId, UpdateLineCountCommandDTO updateLineCountCommandDTO)
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            string payload = JsonSerializer.Serialize(new
            {
                UserId = userId.ToString(),
                Frame = Hex.TwoDigit(updateLineCountCommandDTO.Frame),
                NewLineCount = updateLineCountCommandDTO.NewLineCount.ToString(),
                UpdatedOnUTC = DateTime.UtcNow.ToString()
            });

            IDatabase db = _redis.GetDatabase();

            string? result = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.LineCountUpdateScript,
                [
                    connectionKey
                ],
                [
                    payload
                ]
            );

            if (result == null || !Guid.TryParse(result, out Guid wipId))
            {
                return null;
            }

            return wipId;
        }

        /// <inheritdoc/>
        public async Task<Guid?> UpdatePitchAsync(Guid userId, string connectionId, UpdatePitchCommandDTO updatePitchCommandDTO)
        {
            var cellAddress = CellAddress.CreatePitchAddress(
                updatePitchCommandDTO.Frame,
                updatePitchCommandDTO.Channel,
                updatePitchCommandDTO.Line,
                updatePitchCommandDTO.NoteGroup
            );

            return await UpdateLineAsync(userId, connectionId, TrackerCommands.Pitch, cellAddress, updatePitchCommandDTO.NewPitch);
        }

        /// <inheritdoc/>
        public async Task<Guid?> UpdateInstrumentAsync(Guid userId, string connectionId, UpdateInstrumentCommandDTO updateInstrumentCommandDTO)
        {
            var cellAddress = CellAddress.CreateInstrumentAddress(
                updateInstrumentCommandDTO.Frame,
                updateInstrumentCommandDTO.Channel,
                updateInstrumentCommandDTO.Line,
                updateInstrumentCommandDTO.NoteGroup
            );

            return await UpdateLineAsync(userId, connectionId, TrackerCommands.Instrument, cellAddress, updateInstrumentCommandDTO.NewInstrument);
        }

        public async Task<Guid?> UpdateFXSymbolAsync(Guid userId, string connectionId, UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO)
        {
            var cellAddress = CellAddress.CreateFXSymbolAddress(
                updateFXSymbolCommandDTO.Frame,
                updateFXSymbolCommandDTO.Channel,
                updateFXSymbolCommandDTO.Line,
                updateFXSymbolCommandDTO.FXGroup
            );

            return await UpdateLineAsync(userId, connectionId, TrackerCommands.FXSymbol, cellAddress, updateFXSymbolCommandDTO.NewFXSymbol);
        }

        public async Task<Guid?> UpdateFXValueAsync(Guid userId, string connectionId, UpdateFXValueCommandDTO updateFXValueCommandDTO)
        {
            var cellAddress = CellAddress.CreateFXValueAddress(
                updateFXValueCommandDTO.Frame,
                updateFXValueCommandDTO.Channel,
                updateFXValueCommandDTO.Line,
                updateFXValueCommandDTO.FXGroup
            );

            return await UpdateLineAsync(userId, connectionId, TrackerCommands.FXValue, cellAddress, updateFXValueCommandDTO.NewFXValue);
        }

        private async Task<Guid?> UpdateLineAsync(
            Guid userId,
            string connectionId, 
            string commandType, 
            CellAddress cellAddress,
            byte? newValue
            )
        {
            string connectionKey = CacheKeyBuilder.ConnectionKey(userId, connectionId);

            string payload = JsonSerializer.Serialize(new
            {
                UserId = userId.ToString(),
                Type = commandType,
                Channel = cellAddress.ChannelHex,
                Line = cellAddress.LineHex,
                Column = cellAddress.Column,
                Address = cellAddress.Address,
                NewValue = newValue == null ? NullColumn : Hex.TwoDigit(newValue.Value),
                UpdatedOnUTC = DateTime.UtcNow.ToString()
            });

            IDatabase db = _redis.GetDatabase();

            string? result = (string?)await db.ScriptEvaluateAsync(
                LuaScripts.LineUpdateScript,
                [
                    connectionKey,
                ],
                [
                    payload
                ]
            );

            if (result == null || !Guid.TryParse(result, out Guid wipId))
            {
                return null;
            }

            return wipId;
        }
    }
}
