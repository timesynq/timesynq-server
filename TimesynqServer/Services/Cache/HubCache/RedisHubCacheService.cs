using StackExchange.Redis;
using TimesynqServer.Models.Cache;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using TimesynqServer.Hubs.TrackerHub.Const;

namespace TimesynqServer.Services.Cache.HubCache
{
    public class RedisHubCacheService : IHubCacheService
    {

        private IConnectionMultiplexer _redis;

        public RedisHubCacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(stringResult!);          
        }

        public async Task<bool> SetAsync<T>(string key, T value) where T : class
        {
            IDatabase db = _redis.GetDatabase();
            string serializedValue = JsonSerializer.Serialize(value);

            bool isWriteSuccessful = await db.StringSetAsync(key, serializedValue);

            return isWriteSuccessful;
        }

        public async Task<bool> RemoveAsync(string key)
        {
            IDatabase db = _redis.GetDatabase();

            bool isRemoveSuccessful = await db.KeyDeleteAsync(key);

            return isRemoveSuccessful;
        }

        public async Task<Connection?> GetConnectionAsync(string key, string roomCode)
        {
            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            Connection? connection = JsonSerializer.Deserialize<Connection?>(stringResult!);
            if(connection == null || connection.RoomCode != roomCode)
            {
                return null;
            }

            return connection;
        }

        public async Task<Room?> GetRoomAsync(string key, Guid ownerId)
        {
            IDatabase db = _redis.GetDatabase();
            string? stringResult = await db.StringGetAsync(key);

            if (stringResult.IsNullOrEmpty())
            {
                return null;
            }

            Room? room = JsonSerializer.Deserialize<Room?>(stringResult!);
            if(room == null || room.OwnerId != ownerId)
            {
                return null;
            }

            return room;
        }

        public async Task<bool> SetConnectionAsync(string key, Connection connection)
        {
            IDatabase db = _redis.GetDatabase();
            ITransaction tran = db.CreateTransaction();
            string serializedConnection = JsonSerializer.Serialize(connection);

            _ = await tran.StringSetAsync(key, serializedConnection);
            _ = await tran.SetAddAsync($"{TrackerHubCachePrefixes.Room}:{connection.RoomCode}:{TrackerHubCachePrefixes.Connections}", $"{connection.UserId}");

            bool isTransactionSuccessful = await tran.ExecuteAsync();

            return isTransactionSuccessful;
        }

        public async Task<bool> CloseRoom(string roomCode)
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
