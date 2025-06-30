using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.Cache;
using TimesynqServer.Services;
using TimesynqServer.Services.Cache;
using TimesynqServer.Services.Cache.HubCache;

namespace TimesynqServer.Hubs.TrackerHub
{
    [Authorize]
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
            await _hubCacheService.RemoveAsync($"connection:{callerGuid}");

            string roomCode = connection.RoomCode;
            TimesynqUser? user = await _dbContext.Users.FindAsync(callerGuid);
            if(user == null)
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
            if (ownerConnection == null) 
            {
                await Clients.Group(roomCode).SendAsync(TrackerHubClientCallbacks.DisbandRoom);
                await _hubCacheService.RemoveByPrefix($"room:{roomCode}");

                IEnumerable<Connection> closedRoomConnections = await _hubCacheService.GetRoomConnectionsAsync(roomCode);
                foreach(var connectionToClosedRoom in closedRoomConnections)
                {
                    await _hubCacheService.RemoveAsync($"connection:{connectionToClosedRoom.UserId}");
                }
            }
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

            await Clients.Group(ownedRoom.RoomCode).SendAsync(TrackerHubClientCallbacks.DisbandRoom);
            await _hubCacheService.RemoveAsync($"room:{ownedRoom.RoomCode}");
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

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            //todo: tracker info initialization

            var newRoom = new Room
            { 
                RoomCode = roomCode,
                OwnerId = callerGuid,
            };

            await _hubCacheService.SetAsync($"room:{roomCode}", newRoom);

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

            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);

            var newConnection = new Connection
            {
                ConnectionId = Context.ConnectionId,
                RoomCode = roomCode,
                UserId = callerGuid,
            };

            await _hubCacheService.SetAsync($"connection:{callerGuid}", newConnection);

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
