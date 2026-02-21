namespace TimesynqServer.Infrastructure.Hubs.TrackerHub
{
    internal static class TrackerHubError
    {
        public static string UserNotFound = "Failed to read user information.";
        public static string AlreadyConnectedToARoom = "Already connected to a room. Disband/leave the room and try again.";
        public static string FailedToJoinRoom = "Failed to join room.";
        public static string NotConnected = "Not connected to a room.";
        public static string FailedToReadRoom = "Failed to read room information.";
        public static string FailedToLeaveRoom = "Failed to leave room.";
        public static string WipNotFound = "Wip not found.";
    }
}
