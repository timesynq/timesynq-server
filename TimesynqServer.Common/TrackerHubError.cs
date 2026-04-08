namespace TimesynqServer.Common
{
    public static class TrackerHubError
    {
        public static string UserNotFound { get; } = "Failed to read user information.";
        public static string FailedToJoinRoom { get; } = "Failed to join room.";
        public static string FailedToLeaveRoom { get; } = "Failed to leave room.";
        public static string NoConnectionFound { get; } = "No connection found.";
        public static string WipNotFound { get; } = "Wip not found.";
        public static string FailedToSendMessage { get; } = "Failed to send message. Please try again later.";
        public static string FailedToUpdateBpm { get; } = "Failed to update BPM.";
        public static string InvalidMessage { get; } = $"Invalid message. Message must be between {TrackerHubConstants.MinMessageLength} and {TrackerHubConstants.MaxMessageLength} characters long.";
        public static string InvalidBpm { get; } = $"Invalid BPM. BPM must be between {TrackerConstants.MinBpm} and {TrackerConstants.MaxBpm} inclusive.";
        public static string FailedToUpdateLineCount { get; } = "Failed to update line count.";
        public static string FailedToUpdateLinesPerBeat { get; } = "Failed to update LPB";
        public static string InvalidLineCount { get; } = $"Invalid line count. Line count must be between {TrackerConstants.MinLinesPerFrame} and {TrackerConstants.MaxLinesPerFrame} inclusive.";
        public static string InvalidLinesPerBeat { get; } = $"Invalid lines per beat. Lines per beat must be between {TrackerConstants.MinLinesPerBeat} and {TrackerConstants.MaxLinesPerBeat} inclusive.";
        public static string InvalidFrame { get; } = $"Invalid frame. Frame value must be between 0 and {TrackerConstants.MaxFramesPerWip - 1} inclusive.";
        public static string InvalidChannel { get; } = $"Invalid channel. Channel value must be between 0 and {TrackerConstants.MaxChannels - 1} inclusive.";
        public static string InvalidChannelCount { get; } = $"Invalid channel count. Channel count must be between {TrackerConstants.MinChannels - 1} and {TrackerConstants.MaxChannels - 1} inclusive.";
        public static string InvalidSequencerLength { get; } = $"Invalid sequencer length. Sequencer length must be between {TrackerConstants.MinSequencerLines} and {TrackerConstants.MaxSequencerLines} inclusive.";
        public static string InvalidNoteGroup { get; } = $"Invalid note group. Note group value must be between 0 and {TrackerConstants.MaxNoteGroups - 1} inclusive.";
        public static string InvalidFXGroup { get; } = $"Invalid FX group. FX group value must be between 0 and {TrackerConstants.MaxFXGroups - 1} inclusive.";
        public static string InvalidPitch { get; } = $"Invalid pitch. Pitch value must be between 0 and {TrackerConstants.MaxPitches - 1} inclusive.";
        public static string FailedToUpdatePitch { get; } = "Failed to update pitch.";
        public static string FailedToUpdateInstrument { get; } = "Failed to update instrument";
        public static string FailedToUpdateFXSymbol { get; } = "Failed to update FX symbol";
        public static string FailedToUpdateFXValue { get; } = "Failed to update FX value";
        public static string FailedToUpdateChannelCount { get; } = "Failed to update channel count";
        public static string FailedToUpdateChannelType { get; } = "Failed to update channel type";
        public static string FailedToUpdateSequencerLength { get; } = "Failed to update sequencer length";
        public static string FailedToUpdateSequencerFrame { get; } = "Failed to update sequencer frame";
        public static string FailedToUpdateSequencerChannel { get; } = "Failed to update sequencer channel";
    }
}
