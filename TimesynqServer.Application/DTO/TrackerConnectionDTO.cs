using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents a user connection to a SignalR room.
    /// </summary>
    /// <remarks>
    /// This model is used to transfer cache layer connection data from application to public layer. 
    /// </remarks>
    public sealed class TrackerConnectionDTO
    {
        /// <summary>
        /// The connected user's unique identifier.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// The SignalR ConnectionId associated with the user's connection
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// The code identifying the room the user is connected to.
        /// </summary>
        public Guid WipId { get; }

        private TrackerConnectionDTO(Guid userId, string connectionId, Guid wipId)
        {
            UserId = userId;
            ConnectionId = connectionId;
            WipId = wipId;
        }

        public static TrackerConnectionDTO FromDomainModel(TrackerConnection trackerConnection)
        {
            ArgumentNullException.ThrowIfNull(trackerConnection);

            return new TrackerConnectionDTO(
                trackerConnection.UserId,
                trackerConnection.ConnectionId,
                trackerConnection.WipId
            );
        }
    }
}
