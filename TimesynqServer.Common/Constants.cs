namespace TimesynqServer.Common
{
    public class UserConstants
    {
        public const int MinUserNameLength = 3;
        public const int MaxUserNameLength = 24;
        public const int MinPasswordLength = 12;
        public const int UserNameChangeCooldownDays = 30;
    }

    public class PaginationConstants
    {
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 50;
        public const string DefaultSortOrder = "default";

        public const string DefaultUserSearchSortBy = "username";
    }

    public class TrackerHubConstants
    {
        public const int SecondsBeforeRoomClose = 30;
    }
}
