using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
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

        public TrackerHub(ITrackerHubCache hubCacheService, IUserService userService)
        {
            _trackerHubCache = hubCacheService;
            _userService = userService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            Guid callerId = Guid.Parse(Context.UserIdentifier);

            TrackerConnection? connection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (connection == null)
            {
                return;
            }
            await _trackerHubCache.RemoveConnectionAsync(callerId, connection.RoomCode);

            string roomCode = connection.RoomCode;
            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (userDTO == null)
            {
                return;
            }

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.RemoveProfilePicture, userDTO);

            Room? ownedRoom = await _trackerHubCache.GetRoomAsync(roomCode, userDTO.Id);
            if (ownedRoom == null)
            {
                return;
            }

            //if the user that disconnected is the owner of a room, wait 30s before closing the room
            await Task.Delay(TrackerHubConstants.SecondsBeforeRoomClose * 1000);

            TrackerConnection? ownerConnection = await _trackerHubCache.GetConnectionAsync(callerId, roomCode);
            if (ownerConnection != null)
            {
                return;
            }

            bool isRoomClosed = await _trackerHubCache.RemoveRoomAsync(roomCode);
            if (!isRoomClosed)
            {
                return;
            }

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.DisbandRoom);
        }

        public async Task<TrackerHubResult> DisbandRoom(string roomCode)
        {
            if (Context.UserIdentifier == null)
            {
                return TrackerHubResult.Failure(TrackerHubError.UserNotFound);
            }

            Guid callerId = Guid.Parse(Context.UserIdentifier);

            Room? ownedRoom = await _trackerHubCache.GetRoomAsync(roomCode, callerId);
            if (ownedRoom == null)
            {
                return TrackerHubResult.Failure(TrackerHubError.NotAnOwner);
            }

            bool isRoomClosed = await _trackerHubCache.RemoveRoomAsync(roomCode);
            if (!isRoomClosed)
            {
                return TrackerHubResult.Failure(TrackerHubError.FailedToCloseRoom);
            }

            await Clients.Group(ownedRoom.RoomCode).SendAsync(TrackerHubClientCallbacks.DisbandRoom);
            return TrackerHubResult.Success();
        }

        public async Task<TrackerHubResult<string>> CreateRoom(Guid? wipId)
        {
            if (Context.UserIdentifier == null)
            {
                return TrackerHubResult<string>.Failure(TrackerHubError.UserNotFound);
            }

            Guid callerId = Guid.Parse(Context.UserIdentifier);

            TrackerConnection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (existingConnection != null)
            {
                return TrackerHubResult<string>.Failure(TrackerHubError.AlreadyConnectedToARoom);
            }

            string roomCode = TimesynqRandomizer.GenerateRoomCode();
            while (await _trackerHubCache.GetRoomAsync(roomCode) != null)
            {
                roomCode = TimesynqRandomizer.GenerateRoomCode();
            }

            var newRoom = new Room
            {
                RoomCode = roomCode,
                OwnerId = callerId,
            };

            bool isRoomCached = await _trackerHubCache.SetRoomAsync(roomCode, newRoom);
            if (!isRoomCached)
            {
                return TrackerHubResult<string>.Failure(TrackerHubError.FailedToOpenRoom);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            //todo: tracker info initialization

            return roomCode;
        }

        public async Task<TrackerHubResult<object>> JoinRoom(string roomCode)
        {
            if (Context.UserIdentifier == null)
            {
                return TrackerHubResult<object>.Failure(TrackerHubError.UserNotFound);
            }

            Guid callerId = Guid.Parse(Context.UserIdentifier);

            TrackerConnection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (existingConnection != null)
            {
                return TrackerHubResult<object>.Failure(TrackerHubError.AlreadyConnectedToARoom);
            }

            Room? room = await _trackerHubCache.GetRoomAsync(roomCode);
            if (room == null)
            {
                return TrackerHubResult<object>.Failure(TrackerHubError.RoomNotFound);
            }

            var newConnection = new TrackerConnection
            {
                ConnectionId = Context.ConnectionId,
                RoomCode = roomCode,
                UserId = callerId,
            };

            bool isConnectionCached = await _trackerHubCache.SetConnectionAsync(callerId, newConnection);
            if (!isConnectionCached)
            {
                return TrackerHubResult<object>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (userDTO == null)
            {
                return TrackerHubResult<object>.Failure(TrackerHubError.UserNotFound);
            }

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.ReceiveUserInfo, userDTO);
            return new object();
        }

        public async Task<TrackerHubResult> LeaveRoom()
        {
            if (Context.UserIdentifier == null)
            {
                return TrackerHubResult.Failure(TrackerHubError.UserNotFound);
            }

            Guid callerId = Guid.Parse(Context.UserIdentifier);

            TrackerConnection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerId);
            if (existingConnection == null)
            {
                return TrackerHubResult.Failure(TrackerHubError.NotConnected);
            }

            Room? room = await _trackerHubCache.GetRoomAsync(existingConnection.RoomCode);
            if (room != null && room.OwnerId == callerId)
            {
                return TrackerHubResult.Failure(TrackerHubError.OwnerMustDisband);
            }

            bool isLeaveSuccessful = await _trackerHubCache.RemoveConnectionAsync(existingConnection.UserId, existingConnection.RoomCode);
            if (!isLeaveSuccessful)
            {
                return TrackerHubResult.Failure(TrackerHubError.FailedToLeaveRoom);
            }

            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (room != null && userDTO != null)
                await Clients.Group(room.RoomCode).SendAsync(TrackerHubClientCallbacks.UserLeftRoom, userDTO);

            return TrackerHubResult.Success();
        }

    }
}
