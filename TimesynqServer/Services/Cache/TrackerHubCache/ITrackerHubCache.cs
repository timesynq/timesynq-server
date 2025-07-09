using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache.TrackerHubCache
{
    public interface ITrackerHubCache
    {
        public Task<Connection?> GetConnectionAsync(Guid userId);
        public Task<Connection?> GetConnectionAsync(Guid userId, string roomCode);
        public Task<bool> SetConnectionAsync(Guid userId, Connection connection);
        public Task<bool> RemoveConnectionAsync(Guid userId, string roomCode);
        public Task<Room?> GetRoomAsync(string roomCode);
        public Task<Room?> GetRoomAsync(string roomCode, Guid ownerId);
        public Task<bool> SetRoomAsync(string roomCode, Room room);
        public Task<bool> RemoveRoomAsync(string roomCode);
    }
}
