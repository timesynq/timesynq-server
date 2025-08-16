using Microsoft.AspNetCore.Identity;
using TimesynqServer.Common;

namespace TimesynqServer.Infrastructure.Extensions
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"Username '{userName}' is invalid. Username length must be between {UserConstants.MinUserNameLength} and {UserConstants.MaxUserNameLength} characters long."
            };
        }
    }
}
