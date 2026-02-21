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

            TrackerConnection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (existingConnection != null)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.AlreadyConnectedToARoom);
            }

            // todo: this will fetch the joined tracker-init object and check that instead as if that object is null, the wip does not exist
            WipDTO? wipDTO = await _wipService.GetWipAsync(callerId, wipId);
            if (wipDTO == null)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.WipNotFound);
            }

            TrackerHubResult<Room> getOrCreateRoomResult = await _trackerHubCache.GetOrCreateRoomAsync(wipDTO.OwnerId, wipDTO.Id);
            if (!getOrCreateRoomResult.IsSuccessful || (_ = getOrCreateRoomResult.Value) == null)
            {
                return TrackerHubResult<WipDTO>.Failure(getOrCreateRoomResult.ErrorMessage ?? TrackerHubError.FailedToReadRoom);
            }

            var newConnection = new TrackerConnection
            {
                ConnectionId = Context.ConnectionId,
                WipId = wipId,
                UserId = callerId,
            };

            bool isConnectionCached = await _trackerHubCache.SetConnectionAsync(callerId, newConnection);
            if (!isConnectionCached)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            string roomCode = wipId.ToString();
            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (userDTO == null)
            {
                return TrackerHubResult<WipDTO>.Failure(TrackerHubError.UserNotFound);
            }

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

            TrackerConnection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (existingConnection == null)
            {
                return TrackerHubResult.Failure(TrackerHubError.NotConnected);
            }

            TrackerHubResult result = await _trackerHubCache.RemoveConnectionAndCleanupIfEmptyAsync(callerId, existingConnection.WipId);
            if (result.IsSuccessful)
            {
                string roomCode = existingConnection.WipId.ToString();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomCode);
                await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.UserLeftRoom, callerId);
            }

            return result;
        }

    }
}
