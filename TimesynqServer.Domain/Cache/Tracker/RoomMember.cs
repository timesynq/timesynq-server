namespace TimesynqServer.Domain.Cache.Tracker
{
    /// <summary>
    /// Represents an active member in a room.
    /// </summary>
    public class RoomMember
    {
        /// <summary>
        /// The unique identifier of the room that the user joined.
        /// </summary>
        public required Guid WipId { get; init; }

        /// <summary>
        /// The user's unique identifier.
        /// </summary>
        public required Guid UserId { get; init; }

        /// <summary>
        /// The user's unique username.
        /// </summary>
        public required string UserName { get; init; }

        /// <summary>
        /// The SignalR ConnectionId associated with this instance of RoomMember.
        /// One user can have multiple connections, and thus multiple ConnectionIds, to the same room.
        /// </summary>
        public required string ConnectionId { get; init; }
    }
}
