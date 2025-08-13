namespace TimesynqServer.Domain.Cache
{
    /// <summary>
    /// Represents a SignalR room.
    /// </summary>
    /// <remarks>
    /// This model is used to track active SignalR rooms. 
    /// It is serialized and stored in a Redis cache for the lifetime of the room.
    /// </remarks>
    public class Room
    {
        /// <summary>
        /// The room's unique identifier.
        /// </summary>
        public required string RoomCode { get; set; }

        /// <summary>
        /// The ID of the user who created the room and manages its permissions.
        /// </summary>
        public required Guid OwnerId { get; set; }
    }
}
