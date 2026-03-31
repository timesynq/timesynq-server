using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.TrackerCommandDTO;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize(Roles = "ConfirmedUser, Admin")]
    public class TrackerHub : Hub
    {
        private readonly IRoomService _roomService;

        public TrackerHub(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveRoom();
        }

        public async Task<TrackerHubResult<RoomInitializerDTO>> JoinRoom(Guid wipId)
        {
            TrackerHubResult<RoomJoinedDTO> joinRoomResult = await _roomService.JoinRoom(Context.UserIdentifier, Context.ConnectionId, wipId);
            if (!joinRoomResult.IsSuccessful || joinRoomResult.Value == null)
            {
                return TrackerHubResult<RoomInitializerDTO>.Failure(joinRoomResult.ErrorMessage ?? TrackerHubError.FailedToJoinRoom);
            }

            string roomCode = wipId.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserJoinedRoom, joinRoomResult.Value.UserWhoJoined);

            return new RoomInitializerDTO(joinRoomResult.Value.Wip, joinRoomResult.Value.AlreadyPresentMembers);
        }

        public async Task<TrackerHubResult> LeaveRoom()
        {
            TrackerHubResult<TrackerConnectionDTO> leaveRoomResult = await _roomService.LeaveRoom(Context.UserIdentifier, Context.ConnectionId);
            if (!leaveRoomResult.IsSuccessful || leaveRoomResult.Value == null)
            {
                return TrackerHubResult.Failure(leaveRoomResult.ErrorMessage ?? TrackerHubError.FailedToLeaveRoom);
            }

            string roomCode = leaveRoomResult.Value.WipId.ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserLeftRoom, leaveRoomResult.Value);

            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> SendChatMessage(string? message)
        {
            TrackerHubResult<ChatMessageDTO> sendChatMessageResult = await _roomService.SendChatMessage(Context.UserIdentifier, Context.ConnectionId, message);
            if (!sendChatMessageResult.IsSuccessful || sendChatMessageResult.Value == null)
            {
                return TrackerHubResult.Failure(sendChatMessageResult.ErrorMessage ?? TrackerHubError.FailedToSendMessage);
            }

            ChatMessageDTO chatMessageDTO = sendChatMessageResult.Value;
            string roomCode = chatMessageDTO.WipId.ToString();
            await Clients.Group(roomCode).SendAsync(
                TrackerHubClientCallbacks.MessageAddedToChat,
                chatMessageDTO.UserId,
                chatMessageDTO.Message
            );

            return TrackerHubResult.Success();
        }

        public Task<TrackerHubResult> UpdateBpm(int newBpm)
        {
            return UpdateTracker(
                () => _roomService.UpdateBpm(Context.UserIdentifier, Context.ConnectionId, newBpm),
                TrackerHubClientCallbacks.BpmUpdated,
                newBpm,
                TrackerHubError.FailedToUpdateBpm
            );
        }

        public Task<TrackerHubResult> UpdateLineCount(UpdateLineCountCommandDTO updateLineCountCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdateLineCount(Context.UserIdentifier, Context.ConnectionId, updateLineCountCommandDTO),
                TrackerHubClientCallbacks.LineCountUpdated,
                updateLineCountCommandDTO,
                TrackerHubError.FailedToUpdateLineCount
            );
        }

        public Task<TrackerHubResult> UpdateLinesPerBeat(UpdateLinesPerBeatCommandDTO updateLinesPerBeatCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdateLinesPerBeat(Context.UserIdentifier, Context.ConnectionId, updateLinesPerBeatCommandDTO),
                TrackerHubClientCallbacks.LinesPerBeatUpdated,
                updateLinesPerBeatCommandDTO,
                TrackerHubError.FailedToUpdateLinesPerBeat
            );
        }

        public Task<TrackerHubResult> UpdatePitch(UpdatePitchCommandDTO updatePitchCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdatePitch(Context.UserIdentifier, Context.ConnectionId, updatePitchCommandDTO),
                TrackerHubClientCallbacks.PitchUpdated,
                updatePitchCommandDTO,
                TrackerHubError.FailedToUpdatePitch
            );
        }

        public Task<TrackerHubResult> UpdateInstrument(UpdateInstrumentCommandDTO updateInstrumentCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdateInstrument(Context.UserIdentifier, Context.ConnectionId, updateInstrumentCommandDTO),
                TrackerHubClientCallbacks.InstrumentUpdated,
                updateInstrumentCommandDTO,
                TrackerHubError.FailedToUpdateInstrument
            );
        }

        public Task<TrackerHubResult> UpdateFXSymbol(UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdateFXSymbol(Context.UserIdentifier, Context.ConnectionId, updateFXSymbolCommandDTO),
                TrackerHubClientCallbacks.FXSymbolUpdated,
                updateFXSymbolCommandDTO,
                TrackerHubError.FailedToUpdateFXSymbol
            );
        }

        public Task<TrackerHubResult> UpdateFXValue(UpdateFXValueCommandDTO updateFXValueCommandDTO)
        {
            return UpdateTracker(
                () => _roomService.UpdateFXValue(Context.UserIdentifier, Context.ConnectionId, updateFXValueCommandDTO),
                TrackerHubClientCallbacks.FXValueUpdated,
                updateFXValueCommandDTO,
                TrackerHubError.FailedToUpdateFXValue
            );
        }

        private async Task<TrackerHubResult> UpdateTracker<T>(
            Func<Task<TrackerHubResult<Guid>>> roomServiceFunction,
            string trackerHubClientCallback,
            T payload,
            string errorMessageFallback
        )
        {
            TrackerHubResult<Guid> result = await roomServiceFunction();
            if (!result.IsSuccessful)
            {
                return TrackerHubResult.Failure(result.ErrorMessage ?? errorMessageFallback);
            }

            string roomCode = result.Value.ToString();
            await Clients.Group(roomCode).SendAsync(trackerHubClientCallback, payload);
            return TrackerHubResult.Success();
        }
    }
}
