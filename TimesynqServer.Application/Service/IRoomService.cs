using TimesynqServer.Application.DTO;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Application.Service
{
    public interface IRoomService
    {
        public Task<TrackerHubResult<RoomJoinedDTO>> JoinRoom(string? userIdentifier, string connectionId, Guid wipId);
        public Task<TrackerHubResult<TrackerConnectionDTO>> LeaveRoom(string? userIdentifier, string connectionId);
        public Task<TrackerHubResult<ChatMessageDTO>> SendChatMessage(string? userIdentifier, string connectionId, string? message); 
    }
}
