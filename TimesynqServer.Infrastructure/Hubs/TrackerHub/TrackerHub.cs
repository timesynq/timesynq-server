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

        public async Task<TrackerHubResult> UpdateBpm(int newBpm)
        {
            TrackerHubResult<Guid> updateBpmResult = await _roomService.UpdateBpm(Context.UserIdentifier, Context.ConnectionId, newBpm);
            if (!updateBpmResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updateBpmResult.ErrorMessage ?? TrackerHubError.FailedToUpdateBpm);
            }

            string roomCode = updateBpmResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.BpmUpdated, newBpm);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> UpdateLineCount(UpdateLineCountCommandDTO updateLineCountCommandDTO)
        {
            TrackerHubResult<Guid> updateLineCountResult = await _roomService.UpdateLineCount(Context.UserIdentifier, Context.ConnectionId, updateLineCountCommandDTO);
            if (!updateLineCountResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updateLineCountResult.ErrorMessage ?? TrackerHubError.FailedToUpdateLineCount);
            }

            string roomCode = updateLineCountResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.LineCountUpdated, updateLineCountCommandDTO);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> UpdatePitch(UpdatePitchCommandDTO updatePitchCommandDTO)
        {
            TrackerHubResult<Guid> updatePitchResult = await _roomService.UpdatePitch(Context.UserIdentifier, Context.ConnectionId, updatePitchCommandDTO);
            if (!updatePitchResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updatePitchResult.ErrorMessage ?? TrackerHubError.FailedToUpdatePitch);
            }

            string roomCode = updatePitchResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.PitchUpdated, updatePitchCommandDTO);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> UpdateInstrument(UpdateInstrumentCommandDTO updateInstrumentCommandDTO)
        {
            TrackerHubResult<Guid> updateInstrumentResult = await _roomService.UpdateInstrument(Context.UserIdentifier, Context.ConnectionId, updateInstrumentCommandDTO);
            if (!updateInstrumentResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updateInstrumentResult.ErrorMessage ?? TrackerHubError.FailedToUpdateInstrument);
            }

            string roomCode = updateInstrumentResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.InstrumentUpdated, updateInstrumentCommandDTO);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> UpdateFXSymbol(UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO)
        {
            TrackerHubResult<Guid> updateFXSymbolResult = await _roomService.UpdateFXSymbol(Context.UserIdentifier, Context.ConnectionId, updateFXSymbolCommandDTO);
            if (!updateFXSymbolResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updateFXSymbolResult.ErrorMessage ?? TrackerHubError.FailedToUpdateFXSymbol);
            }

            string roomCode = updateFXSymbolResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.FXSymbolUpdated, updateFXSymbolCommandDTO);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult> UpdateFXValue(UpdateFXValueCommandDTO updateFXValueCommandDTO)
        {
            TrackerHubResult<Guid> updateFXValueResult = await _roomService.UpdateFXValue(Context.UserIdentifier, Context.ConnectionId, updateFXValueCommandDTO);
            if (!updateFXValueResult.IsSuccessful)
            {
                return TrackerHubResult.Failure(updateFXValueResult.ErrorMessage ?? TrackerHubError.FailedToUpdateFXVAlue);
            }

            string roomCode = updateFXValueResult.Value.ToString();
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.FXValueUpdated, updateFXValueCommandDTO);
            return TrackerHubResult.Success();
        }
    }
}
