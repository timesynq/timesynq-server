using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Domain.Cache.Tracker;
using TimesynqServer.Infrastructure.Cache.TrackerHubCache;
using TimesynqServer.Infrastructure.Hubs.TrackerHub;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize(Roles = "ConfirmedUser, Admin")]
    public class TrackerHub : Hub
    {
        private readonly ITrackerHubCache _trackerHubCache;
        private readonly IUserService _userService;
        private readonly IWipService _wipService;

        public TrackerHub(ITrackerHubCache hubCacheService, IUserService userService, IWipService wipService)
        {
            _trackerHubCache = hubCacheService;
            _userService = userService;
            _wipService = wipService;
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
            if (
                Context.UserIdentifier == null ||
                !Guid.TryParse(Context.UserIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.UserNotFound);
            }

            // todo: this will fetch the joined tracker-init object and check that instead as if that object is null, the wip does not exist
            WipDTO? wipDTO = await _wipService.GetWipAsync(callerId, wipId);
            if (wipDTO == null)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.WipNotFound);
            }

            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (userDTO == null)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.UserNotFound);
            }

            var newConnection = new TrackerConnection
            {
                ConnectionId = Context.ConnectionId,
                WipId = wipId,
                UserId = callerId,
            };

            bool isJoinSuccessful = await _trackerHubCache.SetConnectionAndCreateRoomIfEmptyAsync(callerId, newConnection, wipDTO);
            if (!isJoinSuccessful) 
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            string roomCode = wipId.ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserJoinedRoom, userDTO);

            return wipDTO;
        }

        public async Task<TrackerHubResult> LeaveRoom()
        {
            if (
                Context.UserIdentifier == null ||
                !Guid.TryParse(Context.UserIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult.Failure(TrackerHubError.UserNotFound);
            }

            TrackerConnection? trackerConnection = await _trackerHubCache.RemoveConnectionAndCleanupIfEmptyAsync(callerId, Context.ConnectionId);
            if (trackerConnection != null)
            {
                string roomCode = trackerConnection.WipId.ToString();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserLeftRoom, callerId);
            }

            return trackerConnection != null ?
                TrackerHubResult.Success() :
                TrackerHubResult.Failure(TrackerHubError.NoConnectionFound);
        }

    }
}
