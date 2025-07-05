using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Hubs.TrackerHub.Const;
using TimesynqServer.Models.Cache;
using TimesynqServer.Services;
using TimesynqServer.Services.Cache.HubCache;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize(Roles = "ConfirmedUser, Admin")]
    public class TrackerHub : Hub
    {

        private IHubCacheService _hubCacheService;
        private TimesynqDbContext _dbContext;

        public TrackerHub(IHubCacheService hubCacheService, TimesynqDbContext dbContext)
        {
            _hubCacheService = hubCacheService;
            _dbContext = dbContext;
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

            Connection? connection = await _hubCacheService.GetAsync<Connection>($"{TrackerHubCachePrefixes.Connection}:{callerGuid}");
            if (connection == null)
            {
                return;
            }
            await _hubCacheService.RemoveAsync($"{TrackerHubCachePrefixes.Connection}:{callerGuid}");

            string roomCode = connection.RoomCode;
            TimesynqUser? user = await _dbContext.Users.FindAsync(callerGuid);
            if (user == null)
            {
                return;
            }
            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.RemoveProfilePicture, user.ToUserDTO());

            Room? ownedRoom = await _hubCacheService.GetRoomAsync($"{TrackerHubCachePrefixes.Room}:{roomCode}", user.Id);
            if (ownedRoom == null)
            {
                return;
            }

            //if the user that disconnected is the owner of a room, wait 30s before closing the room
            await Task.Delay(30 * 1000);

            Connection? ownerConnection = await _hubCacheService.GetConnectionAsync($"{TrackerHubCachePrefixes.Connection}:{callerGuid}", roomCode);
            if (ownerConnection != null)
            {
                return;
            }

            bool isRoomClosed = await _hubCacheService.CloseRoom(roomCode);
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

            Room? ownedRoom = await _hubCacheService.GetRoomAsync($"{TrackerHubCachePrefixes.Room}:{roomCode}", callerGuid);
            if (ownedRoom == null)
            {
                return;
            }

            bool isRoomClosed = await _hubCacheService.CloseRoom(roomCode);
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

            Connection? existingConnection = await _hubCacheService.GetAsync<Connection>($"{TrackerHubCachePrefixes.Connection}:{callerGuid}");
            if (existingConnection != null)
            {
                return;
            }

            string roomCode = TimesynqRandomizer.GenerateRoomCode();
            while (await _hubCacheService.GetAsync<Room>($"{TrackerHubCachePrefixes.Room}:{roomCode}") != null)
            {
                roomCode = TimesynqRandomizer.GenerateRoomCode();
            }

            var newRoom = new Room
            {
                RoomCode = roomCode,
                OwnerId = callerGuid,
            };

            bool isRoomCached = await _hubCacheService.SetAsync($"{TrackerHubCachePrefixes.Room}:{roomCode}", newRoom);
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

            Connection? existingConnection = await _hubCacheService.GetAsync<Connection>($"{TrackerHubCachePrefixes.Connection}:{callerGuid}");
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

            bool isConnectionCached = await _hubCacheService.SetConnectionAsync($"{TrackerHubCachePrefixes.Connection}:{callerGuid}", newConnection);
            if (!isConnectionCached)
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            TimesynqUser? user = await _dbContext.Users.FindAsync(callerGuid);
            if (user == null)
            {
                return;
            }

            //todo: tracker info initialization

            await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.ReceiveUserInfo, user.ToUserDTO());

        }

    }
}
