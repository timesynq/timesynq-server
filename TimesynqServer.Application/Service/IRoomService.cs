using TimesynqServer.Application.DTO;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Cache.Tracker;

namespace TimesynqServer.Application.Service
{
    public interface IRoomService
    {
        public Task<TrackerHubResult<RoomJoinedDTO>> JoinRoom(string? userIdentifier, string connectionId, Guid wipId);
        public Task<TrackerHubResult<TrackerConnection>> LeaveRoom(string? userIdentifier, string connectionId);
        public Task<TrackerHubResult<ChatMessageDTO>> SendChatMessage(string? userIdentifier, string connectionId, string? message); 
    }
}
