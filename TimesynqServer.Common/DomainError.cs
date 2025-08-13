using Microsoft.AspNetCore.Http;

namespace TimesynqServer.Common
{
    public class DomainError
    {
        public int Code { get; }
        public string Message { get; }

        internal DomainError(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }

    public static class DomainErrors
    {
        public static class User
        {
            public static DomainError NotFound => new(StatusCodes.Status404NotFound, "User not found.");
        }
        public static class Follow
        {
            public static DomainError NotFollowing => new(StatusCodes.Status404NotFound, "Not following this user.");
            public static DomainError CantFollowYourself => new(StatusCodes.Status409Conflict, "Can't follow yourself.");
            public static DomainError AlreadyFollowing => new(StatusCodes.Status409Conflict, "Already following this user.");
        }
    }
}
