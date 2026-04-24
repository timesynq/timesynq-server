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

        public Task<TrackerHubResult<Guid>> UpdateBpm(string? userIdentifier, string connectionId, int newBpm)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateBpmAsync(callerId, connectionId, newBpm),
                () =>
                {
                    if (newBpm < TrackerConstants.MinBpm || newBpm > TrackerConstants.MaxBpm)
                        return TrackerHubError.InvalidBpm;
                    return null;
                }
            );
        }

        public Task<TrackerHubResult<Guid>> UpdateChannelCount(string? userIdentifier, string connectionId, int newChannelCount)
        {
            // the newChannelCount value received from the client does NOT include master channel,
            // so it should be in the range [minChannels-1, maxChannels-1]
            newChannelCount += 1;

            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateChannelCountAsync(callerId, connectionId, newChannelCount),
                () =>
                {
                    if (newChannelCount < TrackerConstants.MinChannels || newChannelCount > TrackerConstants.MaxChannels)
                        return TrackerHubError.InvalidChannelCount;
                    return null;
                }
            );
        }

        public Task<TrackerHubResult<Guid>> UpdateSequencerLength(string? userIdentifier, string connectionId, int newSequencerLength)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateSequencerLengthAsync(callerId, connectionId, newSequencerLength),
                () =>
                {
                    if (newSequencerLength < TrackerConstants.MinSequencerLines || newSequencerLength > TrackerConstants.MaxSequencerLines)
                        return TrackerHubError.InvalidSequencerLength;
                    return null;
                }
            );
        }

        public Task<TrackerHubResult<Guid>> UpdateSequencerFrame(string? userIdentifier, string connectionId, UpdateSequencerFrameCommandDTO updateSequencerFrameCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateSequencerFrameAsync(callerId, connectionId, updateSequencerFrameCommandDTO),
                () => ValidateUpdateSequencerFrameCommand(updateSequencerFrameCommandDTO)
            );

            static string? ValidateUpdateSequencerFrameCommand(UpdateSequencerFrameCommandDTO command)
            {
                if (command.NewFrame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateSequencerChannel(string? userIdentifier, string connectionId, UpdateSequencerChannelCommandDTO updateSequencerChannelCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateSequencerChannelAsync(callerId, connectionId, updateSequencerChannelCommandDTO),
                () => ValidateUpdateSequencerChannelCommand(updateSequencerChannelCommandDTO)
            );

            static string? ValidateUpdateSequencerChannelCommand(UpdateSequencerChannelCommandDTO command)
            {
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidSequencerChannel;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateLineCount(string? userIdentifier, string connectionId, UpdateLineCountCommandDTO updateLineCountCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateLineCountAsync(callerId, connectionId, updateLineCountCommandDTO),
                () =>
                {
                    if (
                        updateLineCountCommandDTO.NewLineCount < TrackerConstants.MinLinesPerFrame ||
                        updateLineCountCommandDTO.NewLineCount > TrackerConstants.MaxLinesPerFrame
                    )
                    {
                        return TrackerHubError.InvalidLineCount;
                    }
                    return null;
                }
            );
        }

        public Task<TrackerHubResult<Guid>> UpdateLinesPerBeat(string? userIdentifier, string connectionId, UpdateLinesPerBeatCommandDTO updateLinesPerBeatCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateLinesPerBeatAsync(callerId, connectionId, updateLinesPerBeatCommandDTO),
                () =>
                {
                    if (
                        updateLinesPerBeatCommandDTO.NewLinesPerBeat < TrackerConstants.MinLinesPerBeat ||
                        updateLinesPerBeatCommandDTO.NewLinesPerBeat > TrackerConstants.MaxLinesPerBeat
                    )
                    {
                        return TrackerHubError.InvalidLinesPerBeat;
                    }
                    return null;
                }
            );
        }

        public Task<TrackerHubResult<Guid>> UpdateChannelType(string? userIdentifier, string connectionId, UpdateChannelTypeCommandDTO updateChannelTypeCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateChannelTypeAsync(callerId, connectionId, updateChannelTypeCommandDTO),
                () => ValidateUpdateChannelTypeCommand(updateChannelTypeCommandDTO)
            );

            static string? ValidateUpdateChannelTypeCommand(UpdateChannelTypeCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateChannelMute(string? userIdentifier, string connectionId, UpdateChannelMuteCommandDTO updateChannelMuteCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateChannelMuteAsync(callerId, connectionId, updateChannelMuteCommandDTO),
                () => ValidateUpdateChannelMuteCommand(updateChannelMuteCommandDTO)
            );

            static string? ValidateUpdateChannelMuteCommand(UpdateChannelMuteCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateChannelSolo(string? userIdentifier, string connectionId, UpdateChannelSoloCommandDTO updateChannelSoloCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateChannelSoloAsync(callerId, connectionId, updateChannelSoloCommandDTO),
                () => ValidateUpdateChannelSoloCommand(updateChannelSoloCommandDTO)
            );

            static string? ValidateUpdateChannelSoloCommand(UpdateChannelSoloCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdatePitch(string? userIdentifier, string connectionId, UpdatePitchCommandDTO updatePitchCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdatePitchAsync(callerId, connectionId, updatePitchCommandDTO),
                () => ValidateUpdatePitchCommand(updatePitchCommandDTO)
            );

            static string? ValidateUpdatePitchCommand(UpdatePitchCommandDTO command) 
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.NoteGroup >= TrackerConstants.MaxNoteGroups)
                    return TrackerHubError.InvalidNoteGroup;
                if (command.NewPitch != null && command.NewPitch >= TrackerConstants.MaxPitches)
                    return TrackerHubError.InvalidPitch;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateInstrument(string? userIdentifier, string connectionId, UpdateInstrumentCommandDTO updateInstrumentCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateInstrumentAsync(callerId, connectionId, updateInstrumentCommandDTO),
                () => ValidateUpdateInstrumentCommand(updateInstrumentCommandDTO)
            );

            static string? ValidateUpdateInstrumentCommand(UpdateInstrumentCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel == TrackerConstants.MasterChannelIndex || command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.NoteGroup >= TrackerConstants.MaxNoteGroups)
                    return TrackerHubError.InvalidNoteGroup;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateFXSymbol(string? userIdentifier, string connectionId, UpdateFXSymbolCommandDTO updateFXSymbolCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateFXSymbolAsync(callerId, connectionId, updateFXSymbolCommandDTO),
                () => ValidateUpdateFXSymbolCommand(updateFXSymbolCommandDTO)
            );

            static string? ValidateUpdateFXSymbolCommand(UpdateFXSymbolCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.FXGroup >= TrackerConstants.MaxFXGroups)
                    return TrackerHubError.InvalidFXGroup;
                return null;
            }
        }

        public Task<TrackerHubResult<Guid>> UpdateFXValue(string? userIdentifier, string connectionId, UpdateFXValueCommandDTO updateFXValueCommandDTO)
        {
            return UpdateTrackerValue(
                userIdentifier,
                callerId => _trackerHubCache.UpdateFXValueAsync(callerId, connectionId, updateFXValueCommandDTO),
                () => ValidateUpdateFXValueCommand(updateFXValueCommandDTO)
            );

            static string? ValidateUpdateFXValueCommand(UpdateFXValueCommandDTO command)
            {
                if (command.Frame >= TrackerConstants.MaxFramesPerWip)
                    return TrackerHubError.InvalidFrame;
                if (command.Channel >= TrackerConstants.MaxChannels)
                    return TrackerHubError.InvalidChannel;
                if (command.FXGroup >= TrackerConstants.MaxFXGroups)
                    return TrackerHubError.InvalidFXGroup;
                return null;
            }
        }

        private static async Task<TrackerHubResult<Guid>> UpdateTrackerValue(
            string? userIdentifier,
            Func<Guid, Task<Guid?>> trackerHubCacheFunction,
            Func<string?>? predicate = null
        )
        {
            if (
                userIdentifier == null ||
                !Guid.TryParse(userIdentifier, out Guid callerId)
                )
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.UserNotFound);
            }

            if (predicate != null)
            {
                string? error = predicate();
                if (error != null)
                {
                    return TrackerHubResult<Guid>.Failure(error);
                }
            }

            Guid? wipId = await trackerHubCacheFunction(callerId);
            if (wipId == null)
            {
                return TrackerHubResult<Guid>.Failure(TrackerHubError.NoConnectionFound);
            }

            return wipId.Value;
        }
    }
}
