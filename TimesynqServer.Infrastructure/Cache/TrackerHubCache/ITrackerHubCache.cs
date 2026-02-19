using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache
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
        /// A task representing the asynchronous operation. The task result contains the user's <see cref="TrackerConnection"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public Task<TrackerConnection?> GetConnectionAsync(Guid userId);

        /// <summary>
        /// Retrieves the connection information for a specified user in a specific room.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains the user's <see cref="TrackerConnection"/> if found; otherwise <c>null</c>.
        /// </returns>
        public Task<TrackerConnection?> GetConnectionAsync(Guid userId, Guid wipId);

        /// <summary>
        /// Stores the connection information for a specified user.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="connection">The <see cref="TrackerConnection"/> object to associate with the user.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> SetConnectionAsync(Guid userId, TrackerConnection connection);

        /// <summary>
        /// Removes the connection information for a specified user in a specific room.
        /// </summary>
        /// <param name="userId">The connected user's unique identifier.</param>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> RemoveConnectionAsync(Guid userId, Guid wipId);

        /// <summary>
        /// Retrieves the room information for a given room code.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains the <see cref="Room"/> if found; otherwise, <c>null</c>.
        /// </returns>
        public Task<Room?> GetRoomAsync(Guid wipId);

        /// <summary>
        /// Retrieves the number of users in a room for a given room code.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result contains the number of users in the room if it exists; otherwise, <c>0</c>.
        /// </returns>
        public Task<int> GetRoomUserCountAsync(Guid wipId);

        /// <summary>
        /// Stores the room information for a given room code.
        /// </summary>
        /// <param name="room">The <see cref="Room"/> object to store.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> SetRoomAsync(Room room);

        /// <summary>
        /// Removes the room and all connections to it from the cache using the given room code.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> RemoveRoomAsync(Guid wipId);
    }
}
