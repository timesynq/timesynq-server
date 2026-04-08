namespace TimesynqServer.Hubs.TrackerHub
{
    /// <summary>
    /// Provides standardized client method names.
    /// </summary>
    public static class TrackerHubClientCallbacks
    {
        public const string UserJoinedRoom = nameof(UserJoinedRoom);
        public const string UserLeftRoom = nameof(UserLeftRoom);
        public const string MessageAddedToChat = nameof(MessageAddedToChat);
        public const string AccessExpired = nameof(AccessExpired);
        public const string WipNameChanged = nameof(WipNameChanged);
        public const string BpmUpdated = nameof(BpmUpdated);
        public const string ChannelCountUpdated = nameof(ChannelCountUpdated);
        public const string SequencerLengthUpdated = nameof(SequencerLengthUpdated);
        public const string SequencerFrameUpdated = nameof(SequencerFrameUpdated);
        public const string SequencerChannelUpdated = nameof(SequencerChannelUpdated);
        public const string LineCountUpdated = nameof(LineCountUpdated);
        public const string LinesPerBeatUpdated = nameof(LinesPerBeatUpdated);
        public const string ChannelTypeUpdated = nameof(ChannelTypeUpdated);
        public const string PitchUpdated = nameof(PitchUpdated);
        public const string InstrumentUpdated = nameof(InstrumentUpdated);
        public const string FXSymbolUpdated = nameof(FXSymbolUpdated);
        public const string FXValueUpdated = nameof(FXValueUpdated);
    }
}
