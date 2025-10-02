using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesynqServer.Domain.Cache
{
    /// <summary>
    /// Represents a user connection to a SignalR hub.
    /// </summary>
    /// <remarks>
    /// This model is used to track active SignalR connections to hubs. 
    /// It is serialized and stored in a Redis cache for the lifetime of the connection.
    /// </remarks>
    public class Connection
    {
        /// <summary>
        /// The connected user's unique identifier.
        /// </summary>
        public required Guid UserId { get; set; }

        /// <summary>
        /// The SignalR ConnectionId associated with the user's connection
        /// </summary>
        public required string ConnectionId { get; set; }
    }
}
