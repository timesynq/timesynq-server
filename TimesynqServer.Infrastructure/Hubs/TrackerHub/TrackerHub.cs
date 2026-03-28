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
    }
}
