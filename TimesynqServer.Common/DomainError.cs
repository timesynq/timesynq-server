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
            public static DomainError InvalidUserName => new(StatusCodes.Status400BadRequest, "Username length must be between 3 and 24 characters long.");
            public static DomainError UserNameChangeOnCooldown => new(StatusCodes.Status400BadRequest, "Username change is on cooldown.");
            public static DomainError NotFound => new(StatusCodes.Status404NotFound, "User not found.");
            public static DomainError UserNameConflict => new(StatusCodes.Status409Conflict, "Already using username.");
            public static DomainError UserNameTaken => new(StatusCodes.Status409Conflict, "Username already taken.");
            public static DomainError AccountAlreadyDeleted => new(StatusCodes.Status409Conflict, "Account has already been deleted.");
        }

        public static class Follow
        {
            public static DomainError NotFollowing => new(StatusCodes.Status404NotFound, "Not following this user.");
            public static DomainError CantFollowYourself => new(StatusCodes.Status409Conflict, "Can't follow yourself.");
            public static DomainError AlreadyFollowing => new(StatusCodes.Status409Conflict, "Already following this user.");
        }

        public static class Wip
        {
            public static DomainError InvalidName => new(StatusCodes.Status400BadRequest, "Name must be between 1 and 100 characters long.");
            public static DomainError NotFound => new(StatusCodes.Status404NotFound, "Wip not found.");
            public static DomainError NameConflict => new(StatusCodes.Status409Conflict, "Already using name.");
            public static DomainError WipDeleted => new(StatusCodes.Status409Conflict, "WIP has already been deleted.");
            public static DomainError NameContainsInvalidCharacters(string characters) => new(StatusCodes.Status400BadRequest, $"Name contains invalid characters: {characters}");
        }
    }
}
