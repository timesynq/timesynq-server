using System.Text.RegularExpressions;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Cache.Tracker;
using TimesynqServer.Hubs.TrackerHub;
using TimesynqServer.Infrastructure.Cache.TrackerHubCache;

namespace TimesynqServer.Infrastructure.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUserService _userService;
        private readonly IWipService _wipService;
        private readonly ITrackerHubCache _trackerHubCache;

        public RoomService(IUserService userService, IWipService wipService, ITrackerHubCache trackerHubCache) 
        {
            _userService = userService;
            _wipService = wipService;
            _trackerHubCache = trackerHubCache;
        }

        public async Task<TrackerHubResult<RoomJoinedDTO>> JoinRoom(string? userIdentifier, string connectionId, Guid wipId)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(TrackerHubError.UserNotFound);
            }

            // todo: this will fetch the joined tracker-init object and check that instead as if that object is null, the wip does not exist
            WipDTO? wipDTO = await _wipService.GetWipAsync(callerId, wipId);
            if (wipDTO == null)
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(TrackerHubError.WipNotFound);
            }

            UserDTO? userDTO = await _userService.GetUserAsync(callerId);
            if (userDTO == null)
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(TrackerHubError.UserNotFound);
            }

            var newConnection = new TrackerConnection
            {
                ConnectionId = connectionId,
                WipId = wipId,
                UserId = callerId,
            };

            bool isJoinSuccessful = await _trackerHubCache.SetConnectionAndCreateRoomIfEmptyAsync(callerId, newConnection, wipDTO);
            if (!isJoinSuccessful)
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(TrackerHubError.FailedToJoinRoom);
            }

            return new RoomJoinedDTO(userDTO, wipDTO);
        }

        public async Task<TrackerHubResult<TrackerConnection>> LeaveRoom(string? userIdentifier, string connectionId)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<TrackerConnection>.Failure(TrackerHubError.UserNotFound);
            }

            TrackerConnection? trackerConnection = await _trackerHubCache.RemoveConnectionAndCleanupIfEmptyAsync(callerId, connectionId);
            return trackerConnection ?? TrackerHubResult<TrackerConnection>.Failure(TrackerHubError.NoConnectionFound);
        }
    }
}
