using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Cache.Tracker;
using TimesynqServer.Infrastructure.Cache.TrackerHubCache;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize(Roles = "ConfirmedUser, Admin")]
    public class TrackerHub : Hub
    {
        private readonly IRoomService _roomService;
        private readonly ITrackerHubCache _trackerHubCache;

        public TrackerHub(IRoomService roomService, ITrackerHubCache hubCacheService)
        {
            _roomService = roomService;
            _trackerHubCache = hubCacheService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await LeaveRoom();
        }

        public async Task<TrackerHubResult<WipDTO>> JoinRoom(Guid wipId)
        {
            TrackerHubResult<RoomJoinedDTO> joinRoomResult = await _roomService.JoinRoom(Context.UserIdentifier, Context.ConnectionId, wipId);
            if (!joinRoomResult.IsSuccessful || joinRoomResult.Value == null)
            {
                return TrackerHubResult<WipDTO>.Failure(joinRoomResult.ErrorMessage ?? TrackerHubError.FailedToJoinRoom);
            }

            if (joinRoomResult.IsSuccessful)
            {
                string roomCode = wipId.ToString();
                await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserJoinedRoom, joinRoomResult.Value.UserDTO);
            }

            return joinRoomResult.Value.WipDTO;
        }

        public async Task<TrackerHubResult> LeaveRoom()
        {
            TrackerHubResult<TrackerConnection> leaveRoomResult = await _roomService.LeaveRoom(Context.UserIdentifier, Context.ConnectionId);
            if (!leaveRoomResult.IsSuccessful || leaveRoomResult.Value == null)
            {
                return TrackerHubResult.Failure(leaveRoomResult.ErrorMessage ?? TrackerHubError.FailedToLeaveRoom);
            }

            if (leaveRoomResult.Value != null)
            {
                string roomCode = leaveRoomResult.Value.WipId.ToString();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserLeftRoom, leaveRoomResult.Value.UserId);
            }

            return TrackerHubResult.Success();
        }
    }
}
