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
    }
}
