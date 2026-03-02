using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents an active member in a room.
    /// </summary>
    /// <remarks>
    /// This model is used to transfer cache layer connection data from application to public layer. 
    /// </remarks>
    public class RoomMemberDTO
    {
        /// <summary>
        /// The user's unique identifier.
        /// </summary>
        public Guid UserId { get; }

        /// <summary>
        /// The user's unique username.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The SignalR ConnectionId associated with this instance of RoomMember.
        /// One user can have multiple connections, and thus multiple ConnectionIds, to the same room.
        /// </summary>
        public string ConnectionId { get; }

        public RoomMemberDTO(UserDTO userDTO, string connectionId)
            : this(userDTO.Id, userDTO.UserName, connectionId) {}

        private RoomMemberDTO(Guid userId, string userName, string connectionId) 
        {
            UserId = userId;
            UserName = userName;
            ConnectionId = connectionId;
        }

        public static RoomMemberDTO FromDomainModel(RoomMember roomMember)
        {
            return new RoomMemberDTO(
                roomMember.UserId,
                roomMember.UserName,
                roomMember.ConnectionId
            );
        }
    }
}
