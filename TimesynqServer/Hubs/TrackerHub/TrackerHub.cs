using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.Cache;
using TimesynqServer.Models.DTO;
using TimesynqServer.Services.Cache.TrackerHubCache;
using TimesynqServer.Services.Repository.UserRepository;
using TimesynqServer.Services.Static;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize(Roles = "ConfirmedUser, Admin")]
    public class TrackerHub : Hub
    {
        private readonly ITrackerHubCache _trackerHubCache;
        private readonly IUserRepository _userRepository;

        public TrackerHub(ITrackerHubCache hubCacheService, IUserRepository userRepository)
        {
            _trackerHubCache = hubCacheService;
            _userRepository = userRepository;
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
            TimesynqUser? user = await _userRepository.GetById(callerGuid);
            if (user == null)
            {
                return;
            }
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.RemoveProfilePicture, user.ToUserDTO());

            Room? ownedRoom = await _trackerHubCache.GetRoomAsync(roomCode, user.Id);
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

            TimesynqUser? user = await _userRepository.GetById(callerGuid);
            if (user == null)
            {
                return;
            }

            //todo: tracker info initialization

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.ReceiveUserInfo, user.ToUserDTO());

        }

    }
}
