using Microsoft.AspNetCore.Identity;

namespace TimesynqServer.Extensions
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError InvalidUserName(string? userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"Username '{userName}' is invalid. Username length must be between 3 and 24 characters long."
            };
        }
    }
}
