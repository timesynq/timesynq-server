using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Contracts.TrackerCommandDTO;
using TimesynqServer.Domain.Cache.Tracker;
using TimesynqServer.Domain.Entities.Shares;
using TimesynqServer.Infrastructure.Cache.TrackerHubCache;

namespace TimesynqServer.Infrastructure.Service
{
    public class RoomService : IRoomService
    {
        private readonly IUserService _userService;
        private readonly IWipService _wipService;
        private readonly IShareRepository _shareRepository;
        private readonly ITrackerHubCache _trackerHubCache;

        public RoomService(IUserService userService, IWipService wipService, IShareRepository shareRepository, ITrackerHubCache trackerHubCache) 
        {
            _userService = userService;
            _wipService = wipService;
            _shareRepository = shareRepository;
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
            // for initial manual testing sake this is fine, but these two calls should be combined into one "GetOwnedOrSharedTracker" method
            WipDTO? wipDTO = await _wipService.GetWipAsync(callerId, wipId);
            SharedWipProjection? sharedWipProjection = await _shareRepository.GetSharedWipAsync(wipId, callerId);
            if (wipDTO == null && sharedWipProjection == null)
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(TrackerHubError.WipNotFound);
            }

            WipDTO loadedWipDTO = wipDTO ?? WipDTO.FromProjection(sharedWipProjection!);

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

            TrackerHubResult<IEnumerable<RoomMember>> joinRoomResult = await _trackerHubCache.SetConnectionAndCreateRoomIfEmptyAsync(userDTO, newConnection, loadedWipDTO);
            if (!joinRoomResult.IsSuccessful || joinRoomResult.Value == null)
            {
                return TrackerHubResult<RoomJoinedDTO>.Failure(joinRoomResult.ErrorMessage ?? TrackerHubError.FailedToJoinRoom);
            }

            return new RoomJoinedDTO(
                loadedWipDTO,
                new RoomMemberDTO(userDTO, connectionId),
                joinRoomResult.Value.Select(RoomMemberDTO.FromDomainModel)
            );
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

        public async Task<TrackerHubResult<Guid>> UpdateBpm(string? userIdentifier, string connectionId, int newBpm)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.UserNotFound);
            }

            if (newBpm < TrackerConstants.MinBpm || newBpm > TrackerConstants.MaxBpm)
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.InvalidBpm);
            }

            Guid? wipId = await _trackerHubCache.UpdateBpmAsync(callerId, connectionId, newBpm);
            if (wipId == null)
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.NoConnectionFound);
            }

            return wipId.Value;
        }

        public async Task<TrackerHubResult<Guid>> UpdatePitch(string? userIdentifier, string connectionId, UpdatePitchCommandDTO updatePitchCommandDTO)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.UserNotFound);
            }

            string? errorMessage = ValidateUpdatePitchCommand(updatePitchCommandDTO);
            if (errorMessage != null) 
            {
                return TrackerHubResult<Guid>.Failure(errorMessage);
            }

            Guid? wipId = await _trackerHubCache.UpdatePitchAsync(callerId, connectionId, updatePitchCommandDTO);
            if (wipId == null)
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.NoConnectionFound);
            }

            return wipId.Value;

            static string? ValidateUpdatePitchCommand(UpdatePitchCommandDTO command) 
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.NoteGroup >= TrackerConstants.MaxNoteGroups)
                    return TrackerHubError.InvalidNoteGroup;
                if (command.NewPitch != null && command.NewPitch >= TrackerConstants.MaxPitches)
                    return TrackerHubError.InvalidPitch;
                return null;
            }
        }

        public async Task<TrackerHubResult<Guid>> UpdateInstrument(string? userIdentifier, string connectionId, UpdateInstrumentCommandDTO updateInstrumentCommandDTO)
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.UserNotFound);
            }

            string? errorMessage = ValidateUpdateInstrumentCommand(updateInstrumentCommandDTO);
            if (errorMessage != null)
            {
                return TrackerHubResult<Guid>.Failure(errorMessage);
            }

            Guid? wipId = await _trackerHubCache.UpdateInstrumentAsync(callerId, connectionId, updateInstrumentCommandDTO);
            if (wipId == null)
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.NoConnectionFound);
            }

            return wipId.Value;

            static string? ValidateUpdateInstrumentCommand(UpdateInstrumentCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.NoteGroup >= TrackerConstants.MaxNoteGroups)
                    return TrackerHubError.InvalidNoteGroup;
                return null;
            }
        }
    }
}
