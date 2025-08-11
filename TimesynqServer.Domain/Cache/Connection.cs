namespace TimesynqServer.Domain.Cache
{
    /// <summary>
    /// Represents a user connection to a SignalR room.
    /// </summary>
    /// <remarks>
    /// This model is used to track active SignalR connections to specific rooms. 
    /// It is serialized and stored in a Redis cache for the lifetime of the connection.
    /// </remarks>
    public class Connection
    {
        /// <summary>
        /// The connected user's unique identifier.
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// The code identifying the room the user is connected to.
        /// </summary>
        public required string RoomCode { get; set; }

        /// <summary>
        /// The SignalR ConnectionId associated with the user's connection
        /// </summary>
        public required string ConnectionId { get; set; }
    }
}
