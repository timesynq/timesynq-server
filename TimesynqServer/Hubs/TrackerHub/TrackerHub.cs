using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service.UserService;
using TimesynqServer.Domain.Cache;
using TimesynqServer.Services.Cache.TrackerHubCache;
using TimesynqServer.Services.Static;

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

            Guid callerGuid = Guid.Parse(Context.UserIdentifier);

            Connection? connection = await _trackerHubCache.GetConnectionAsync(callerGuid);
            if (connection == null)
            {
                return;
            }
            await _trackerHubCache.RemoveConnectionAsync(callerGuid, connection.RoomCode);

            string roomCode = connection.RoomCode;
            UserDTO? userDTO = await _userService.GetUserAsync(callerGuid);
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
            await Task.Delay(30 * 1000);

            Connection? ownerConnection = await _trackerHubCache.GetConnectionAsync(callerGuid, roomCode);
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

        public async Task DisbandRoom(string roomCode)
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            Guid callerGuid = Guid.Parse(Context.UserIdentifier);

            Room? ownedRoom = await _trackerHubCache.GetRoomAsync(roomCode, callerGuid);
            if (ownedRoom == null)
            {
                return;
            }

            bool isRoomClosed = await _trackerHubCache.RemoveRoomAsync(roomCode);
            if (!isRoomClosed)
            {
                return;
            }

            await Clients.Group(ownedRoom.RoomCode).SendAsync(TrackerHubClientCallbacks.DisbandRoom);

        }

        public async Task CreateRoom(Guid? wipId = null)
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            Guid callerGuid = Guid.Parse(Context.UserIdentifier);

            Connection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerGuid);
            if (existingConnection != null)
            {
                return;
            }

            string roomCode = TimesynqRandomizer.GenerateRoomCode();
            while (await _trackerHubCache.GetRoomAsync(roomCode) != null)
            {
                roomCode = TimesynqRandomizer.GenerateRoomCode();
            }

            var newRoom = new Room
            {
                RoomCode = roomCode,
                OwnerId = callerGuid,
            };

            bool isRoomCached = await _trackerHubCache.SetRoomAsync(roomCode, newRoom);
            if (!isRoomCached)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            //todo: tracker info initialization

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.ReceiveRoomCode);

        }

        public async Task JoinRoom(string roomCode)
        {
            if (Context.UserIdentifier == null)
            {
                return;
            }

            Guid callerGuid = Guid.Parse(Context.UserIdentifier);

            Connection? existingConnection = await _trackerHubCache.GetConnectionAsync(callerGuid);
            if (existingConnection != null)
            {
                return;
            }

            var newConnection = new Connection
            {
                ConnectionId = Context.ConnectionId,
                RoomCode = roomCode,
                UserId = callerGuid,
            };

            bool isConnectionCached = await _trackerHubCache.SetConnectionAsync(callerGuid, newConnection);
            if (!isConnectionCached)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            UserDTO? userDTO = await _userService.GetUserAsync(callerGuid);
            if (userDTO == null)
            {
                return;
            }

            //todo: tracker info initialization

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.ReceiveUserInfo, userDTO);

        }

    }
}
