namespace TimesynqServer.Domain.Cache.Tracker
{
    /// <summary>
    /// Represents a user connection to a SignalR room.
    /// </summary>
    /// <remarks>
    /// This model is used to track active SignalR connections to specific rooms. 
    /// It is serialized and stored in a Redis cache for the lifetime of the connection.
    /// </remarks>
    public class TrackerConnection : Connection
    {
        /// <summary>
        /// The code identifying the room the user is connected to.
        /// </summary>
        public required string RoomCode { get; set; }
    }
}
