namespace TimesynqServer.Infrastructure.Hubs.TrackerHub
{
    internal static class TrackerHubError
    {
        public static string UserNotFound = "Failed to read user information.";
        public static string NotAnOwner = "Not an owner of an existing room.";
        public static string FailedToCloseRoom = "Failed to close room.";
        public static string AlreadyConnectedToARoom = "Already connected to a room. Disband/leave the room and try again.";
        public static string FailedToOpenRoom = "Failed to open room.";
        public static string FailedToJoinRoom = "Failed to join room.";
        public static string NotConnected = "Not connected to a room.";
        public static string OwnerMustDisband = "Owner cannot leave a room that they own. Please disband the room instead.";
        public static string FailedToLeaveRoom = "Failed to leave room.";
        public static string RoomNotFound = "Room does not exist.";
    }
}
