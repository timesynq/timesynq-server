using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Cache.Tracker;
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

        public async Task<TrackerHubResult<TrackerConnectionDTO>> LeaveRoom(string? userIdentifier, string connectionId)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<TrackerConnectionDTO>.Failure(TrackerHubError.UserNotFound);
            }

            TrackerConnection? trackerConnection = await _trackerHubCache.RemoveConnectionAndCleanupIfEmptyAsync(callerId, connectionId);
            return trackerConnection != null ?
                TrackerConnectionDTO.FromDomainModel(trackerConnection) : 
                TrackerHubResult<TrackerConnectionDTO>.Failure(TrackerHubError.NoConnectionFound);
        }

        public async Task<TrackerHubResult<ChatMessageDTO>> SendChatMessage(string? userIdentifier, string connectionId, string? message)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<ChatMessageDTO>.Failure(TrackerHubError.UserNotFound);
            }

            Guid? wipId = await _trackerHubCache.GetRoomCodeAsync(callerId, connectionId);
            if (wipId == null)
            {
                return TrackerHubResult<ChatMessageDTO>.Failure(TrackerHubError.NoConnectionFound);
            }

            if 
            (
                string.IsNullOrEmpty(message) ||

                message.Length < TrackerHubConstants.MinMessageLength || 
                message.Length > TrackerHubConstants.MaxMessageLength
            )
            {
                return TrackerHubResult<ChatMessageDTO>.Failure(TrackerHubError.InvalidMessage);
            }

            return new ChatMessageDTO(wipId.Value, callerId, message);
        }
    }
}
