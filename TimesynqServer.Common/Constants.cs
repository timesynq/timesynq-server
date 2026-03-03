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
    }

    public static class TrackerHubConstants
    {
        public const int SecondsBeforeRoomClose = 30;
        public const int MinMessageLength = 1;
        public const int MaxMessageLength = 500;
    }
}
