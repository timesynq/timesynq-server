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
        public static string InvalidMessage = $"Invalid message. Message must be between {TrackerHubConstants.MinMessageLength} and {TrackerHubConstants.MaxMessageLength} characters long.";
        public static string InvalidFrame = $"Invalid frame. Frame value must be between 0 and {TrackerConstants.MaxFramesPerWip - 1} inclusive.";
        public static string InvalidChannel = $"Invalid channel. Channel value must be between 0 and {TrackerConstants.MaxChannels - 1} inclusive.";
        public static string InvalidNoteGroup = $"Invalid note group. Note group value must be between 0 and {TrackerConstants.MaxNoteGroups - 1} inclusive.";
        public static string InvalidFXGroup = $"Invalid FX group. FX group value must be between 0 and {TrackerConstants.MaxFXGroups - 1} inclusive.";
        public static string InvalidPitch = $"Invalid pitch. Pitch value must be between 0 and {TrackerConstants.MaxPitches - 1} inclusive.";
        public static string FailedToUpdatePitch = "Failed to update pitch.";
        public static string FailedToUpdateInstrument = "Failed to update instrument";
    }
}
