namespace TimesynqServer.Hubs.TrackerHub.Const
{
    /// <summary>
    /// Provides standardized client method names.
    /// </summary>
    public static class TrackerHubClientCallbacks
    {
        public const string DisbandRoom = nameof(DisbandRoom);
        public const string RemoveProfilePicture = nameof(RemoveProfilePicture);
        public const string ReceiveRoomCode = nameof(ReceiveRoomCode);
        public const string ReceiveUserInfo = nameof(ReceiveUserInfo);
    }
}
