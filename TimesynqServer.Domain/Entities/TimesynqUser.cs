using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;

namespace TimesynqServer.Domain.Entities
{
    /// <summary>
    /// Represents a user of Timesynq
    /// </summary>
    /// <remarks>
    /// An instance of TimesynqUser should not be returned to the client. 
    /// When user information is returned to the client, convert to a UserDTO with the ToUserDTO extension method.
    /// </remarks>
    public class TimesynqUser : IdentityUser<Guid>
    {
        /// <summary>
        /// A string representation of the user's unique identifier.
        /// </summary>
        public string IdString => Id.ToString();

        /// <summary>
        /// The user's old username in case they want to switch back to it later.
        /// </summary>
        public string SavedUserName { get; private set; } = string.Empty;

        /// <summary>
        /// The normalized version of the user's old username.
        /// </summary>
        public string NormalizedSavedUserName { get; private set; } = string.Empty;

        /// <summary>
        /// A 21 character string representing an identicon.
        /// First 6 chars = hex color;
        /// Next 15 = 5x5 grid (binary), mirrored:
        /// Cols 1=5, 2=4. '1' = color, '0' = white.
        /// </summary>
        public string ProfilePicture { get; } = TimesynqRandomizer.GenerateIdenticon();

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public DateTime CreatedOnUTC { get; } = DateTime.UtcNow;

        /// <summary>
        /// The date when the user's information was last changed.
        /// </summary>
        public DateTime LastUpdatedOnUTC { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// The date when the user last updated their username.
        /// </summary>
        public DateTime LastUpdatedUserNameUTC { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// The date when the user deleted their account.
        /// </summary>
        public DateTime? DeletedOnUTC { get; private set; }

        /// <summary>
        /// Shortcut for determining if a user's account has been deleted.
        /// </summary>
        public bool IsDeleted => DeletedOnUTC.HasValue;

        /// <summary>
        /// Navigation property for all users that are following this user.
        /// </summary>
        public ICollection<Follow> Followers { get; private set; } = [];

        /// <summary>
        /// Navigation property for all users that this user is following.
        /// </summary>
        public ICollection<Follow> Following { get; private set; } = [];

        public Result ChangeUserName(string newUserName)
        {
            if (string.IsNullOrEmpty(newUserName) || newUserName.Length < 3 || newUserName.Length > 24)
            {
                return Result.Failure(DomainErrors.User.InvalidUserName);
            }

            if(newUserName == UserName)
            {
                return Result.Failure(DomainErrors.User.UserNameConflict);
            }

            SavedUserName = UserName!;
            NormalizedSavedUserName = UserName!.ToUpper();
            UserName = newUserName;
            NormalizedUserName = newUserName.ToUpper();

            DateTime now = DateTime.UtcNow;
            LastUpdatedOnUTC = now;
            LastUpdatedUserNameUTC = now;

            return Result.Success();
        }

        public Result SoftDelete()
        {
            if (IsDeleted)
            {
                return Result.Failure(DomainErrors.User.AccountAlreadyDeleted);
            }

            DeletedOnUTC = DateTime.UtcNow;
            return Result.Success();
        }

    }
}
