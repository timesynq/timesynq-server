using TimesynqServer.Application.DTO;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.TrackerCommandDTO;

namespace TimesynqServer.Application.Service
{
    public interface IRoomService
    {
        public Task<TrackerHubResult<RoomJoinedDTO>> JoinRoom(string? userIdentifier, string connectionId, Guid wipId);
        public Task<TrackerHubResult<TrackerConnectionDTO>> LeaveRoom(string? userIdentifier, string connectionId);
        public Task<TrackerHubResult<ChatMessageDTO>> SendChatMessage(string? userIdentifier, string connectionId, string? message);
        public Task<TrackerHubResult<Guid>> UpdateBpm(string? userIdentifier, string connectionId, int newBpm);
        public Task<TrackerHubResult<Guid>> UpdateLineCount(string? userIdentifier, string connectionId, UpdateLineCountCommandDTO updateLineCountCommandDTO);
        public Task<TrackerHubResult<Guid>> UpdatePitch(string? userIdentifier, string connectionId, UpdatePitchCommandDTO updatePitchCommandDTO);
        public Task<TrackerHubResult<Guid>> UpdateInstrument(string? userIdentifier, string connectionId, UpdateInstrumentCommandDTO updateInstrumentCommandDTO);
        public Task<TrackerHubResult<Guid>> UpdateFXSymbol(string? userIdentifier, string connectionId, UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO);
        public Task<TrackerHubResult<Guid>> UpdateFXValue(string? userIdentifier, string connectionId, UpdateFXValueCommandDTO updateFXValueCommandDTO);
    }
}
