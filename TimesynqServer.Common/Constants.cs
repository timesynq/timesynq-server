namespace TimesynqServer.Common
{
    public static class UserConstants
    {
        public const int MinUserNameLength = 3;
        public const int MaxUserNameLength = 24;
        public const int MinPasswordLength = 12;
        public const int UserNameChangeCooldownDays = 30;
    }

    public static class PaginationConstants
    {
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 50;
        public const string DefaultSortOrder = "default";

        public const string DefaultUserSearchSortBy = "username";

        public const string DefaultFollowSearchSortBy = "followers";

        public const string DefaultWipSortBy = "lastopened";

        public const string DefaultShareSortBy = "shareage";
    }

    public static class WipConstants
    {
        public const int MinNameLength = 1;
        public const int MaxNameLength = 100;
        public const int MaxUnacceptedShares = 10;
        public const int MaxUsersAWipCanBeSharedWith = 10;
    }

    public static class TrackerHubConstants
    {
        public const int SecondsBeforeRoomClose = 30;
        public const int MinMessageLength = 1;
        public const int MaxMessageLength = 500;
    }

    public static class TrackerConstants
    {
        public const int MaxFramesPerWip = 64;
        public const int MaxChannels = 16;
        public const int MaxLinesPerChannel = 256;
        public const int MaxNoteGroups = 3;
        public const int MaxFXGroups = 4;
        public const int MaxPitches = 108;
        public const int MinBpm = 20;
        public const int MaxBpm = 999;
        public const int DefaultBpm = 120;
    }
}
