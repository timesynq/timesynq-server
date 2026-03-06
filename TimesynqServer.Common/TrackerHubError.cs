namespace TimesynqServer.Common
{
    public static class TrackerHubError
    {
        public static string UserNotFound = "Failed to read user information.";
        public static string FailedToJoinRoom = "Failed to join room.";
        public static string FailedToLeaveRoom = "Failed to leave room.";
        public static string NoConnectionFound = "No connection found.";
        public static string WipNotFound = "Wip not found.";
        public static string FailedToSendMessage = "Failed to send message. Please try again later.";
        public static string InvalidMessage = $"Invalid message. Message must be between {TrackerHubConstants.MinMessageLength} and {TrackerHubConstants.MaxMessageLength} characters long.$";
    }
}
