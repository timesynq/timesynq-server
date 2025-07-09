using TimesynqServer.Models.Cache;

namespace TimesynqServer.Services.Cache.TrackerHubCache
{

    /// <summary>
    /// Cache interface for managing TrackerHub connections and rooms.
    /// </summary>
    public interface ITrackerHubCache
    {
        /// <summary>
        /// Retrieves the connection information for a specified user.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the user's <see cref="Connection"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public Task<Connection?> GetConnectionAsync(Guid userId);

        /// <summary>
        /// Retrieves teh connection information for a specified user in a specific room.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="roomCode">The room's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the user's <see cref="Connection"/> if found; otherwise <c>null</c>.
        /// </returns>
        public Task<Connection?> GetConnectionAsync(Guid userId, string roomCode);

        /// <summary>
        /// Stores the connection information for a specified user.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="connection">The <see cref="Connection"/> object to associate with the user.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> SetConnectionAsync(Guid userId, Connection connection);

        /// <summary>
        /// Removes the connection information for a specified user in a specific room.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="roomCode">The room's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> RemoveConnectionAsync(Guid userId, string roomCode);

        /// <summary>
        /// Retrieves the room information for a given room code.
        /// </summary>
        /// <param name="roomCode">The room's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains the <see cref="Room"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public Task<Room?> GetRoomAsync(string roomCode);

        /// <summary>
        /// Retrieves the room information for a given room code and owner ID.
        /// </summary>
        /// <param name="roomCode">The room's unique identifier.</param>
        /// <param name="ownerId">The room owner's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains the <see cref="Room"/> if found and owned by the specified user; otherwise, <c>null</c>.
        /// </returns>
        public Task<Room?> GetRoomAsync(string roomCode, Guid ownerId);

        /// <summary>
        /// Stores the room information for a given room code.
        /// </summary>
        /// <param name="roomCode">The code to identify the room.</param>
        /// <param name="room">The <see cref="Room"/> object to store.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> SetRoomAsync(string roomCode, Room room);

        /// <summary>
        /// Removes the room and all connections to it from the cache using the given room code.
        /// </summary>
        /// <param name="roomCode">The room's unique identifier.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> RemoveRoomAsync(string roomCode);
    }
}
