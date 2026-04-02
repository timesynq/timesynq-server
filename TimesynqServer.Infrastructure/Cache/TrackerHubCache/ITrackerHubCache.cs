using TimesynqServer.Application.DTO;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.TrackerCommandDTO;
using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Infrastructure.Cache.TrackerHubCache
{

    /// <summary>
    /// Cache interface for managing TrackerHub connections and rooms.
    /// </summary>
    public interface ITrackerHubCache
    {
        /// <summary>
        /// Get's the room the requesting user is connected to.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> GetRoomCodeAsync(Guid userId, string connectionId);

        /// <summary>
        /// Get's all connection ids associated with the specified room and user combination.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on</param>
        /// <param name="userId">The user's unique identifier.</param>
        /// <returns>An enumerable list of SignalR connection identifiers that correspond to the same user within the same room.</returns>
        public Task<IEnumerable<string>> GetConnectionIdsAsync(Guid wipId, Guid userId);

        /// <summary>
        /// Adds a connection to a room, and initializes the room if it doesn't already exist.
        /// Joining a room will prevent it from expiring.
        /// </summary>
        /// <param name="userDTO">The user's public information.</param>
        /// <param name="connection">The new connection object to be stored in the cache.</param>
        /// <param name="wipDTO">The room initialization information.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<TrackerHubResult<IEnumerable<RoomMember>>> SetConnectionAndCreateRoomIfEmptyAsync(UserDTO userDTO, TrackerConnection connection, WipDTO wipDTO);
     
        /// <summary>
        /// Removes a connection to a room, and sets the room to expire if this was the last person to leave the room.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result is null if there was no connection found matching the input.
        /// The task result is non-null if all operations were completed successfully.
        /// </returns>
        public Task<TrackerConnection?> RemoveConnectionAndCleanupIfEmptyAsync(Guid userId, string connectionId);

        /// <summary>
        /// Removes the room and all connections to it from the cache using the given room code.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> RemoveRoomAsync(Guid wipId);

        /// <summary>
        /// Changes the name of a wip that is stored in the cache.
        /// </summary>
        /// <param name="wipId">The room's unique identifier, taken from the unique identifier of the wip that the room was opened on.</param>
        /// <param name="newName">The new wip name.</param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// The task result indicates whether the operation was successful.
        /// </returns>
        public Task<bool> ChangeWipNameAsync(Guid wipId, string newName);

        /// <summary>
        /// Updates the wip's starting BPM. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="newBpm">The new BPM value.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateBpmAsync(Guid userId, string connectionId, int newBpm);

        /// <summary>
        /// Update's the wip's channel count. Channel counts for each frame are logically capped at this new value. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="newChannelCount">The new channel count.</param>
        /// <returns></returns>
        public Task<Guid?> UpdateChannelCountAsync(Guid userId, string connectionId, int newChannelCount);

        /// <summary>
        /// Updates the line count of a given frame in the wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updateLineCountCommandDTO">The information on which frame to update and the new line count.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateLineCountAsync(Guid userId, string connectionId, UpdateLineCountCommandDTO updateLineCountCommandDTO);

        /// <summary>
        /// Updates the LPB of a given frame int he wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updateLinesPerBeatCommandDTO">THe information on which frame to update and the new LPB.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateLinesPerBeatAsync(Guid userId, string connectionId, UpdateLinesPerBeatCommandDTO updateLinesPerBeatCommandDTO);

        /// <summary>
        /// Updates a single pitch value in a wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updatePitchCommandDTO">The information on what the new pitch is and where it goes.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdatePitchAsync(Guid userId, string connectionId, UpdatePitchCommandDTO updatePitchCommandDTO);

        /// <summary>
        /// Updates a single instrument value in a wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updateInstrumentCommandDTO">The information on what the new instrument is and where it goes.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateInstrumentAsync(Guid userId, string connectionId, UpdateInstrumentCommandDTO updateInstrumentCommandDTO);

        /// <summary>
        /// Updates a single FX symbol in a wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updateFXSymbolCommandDTO">The information on what the new FX symbol is and where it goes.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateFXSymbolAsync(Guid userId, string connectionId, UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO);

        /// <summary>
        /// Updates a single FX value in a wip. User must have permission to edit the wip in order for changes to be made.
        /// </summary>
        /// <param name="userId">The user's unique identifier.</param>
        /// <param name="connectionId">The SignalR connection identifier that corresponds to the request.</param>
        /// <param name="updateFXSymbolCommandDTO">The information on what the new FX value is and where it goes.</param>
        /// <returns>A guid representing the room's unique identifier. Null if no connection is found.</returns>
        public Task<Guid?> UpdateFXValueAsync(Guid userId, string connectionId, UpdateFXValueCommandDTO updateFXValueCommandDTO);
    }
}
