using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities.Follows;

namespace TimesynqServer.Domain.Entities.Users
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
        /// An 11 character string representing an identicon.
        /// The first 6 characters represent a hex color code.
        /// The 7th character is a period delimiter.
        /// The last 4 characters are a hex code encoding 15 bits of information. The most significant bit is always 0, so the highest possible value for the first hex digit is 7.
        /// These 15 bits are decoded into a 5x5 grid (binary), mirrored:
        /// Columns 1 and 2 are mapped to 5 and 4, respectively.
        /// In the grid, '1' represents the color (from the hex code), and '0' represents no color.
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
            if (string.IsNullOrEmpty(newUserName) || newUserName.Length < UserConstants.MinUserNameLength || newUserName.Length > UserConstants.MinUserNameLength)
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
