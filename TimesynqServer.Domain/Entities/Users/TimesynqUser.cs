using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities.Follows;
using TimesynqServer.Domain.Entities.Shares;
using TimesynqServer.Domain.Entities.Wips;

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
        /// An unsigned 32 bit integer, which encodes an identicon
        /// Bit 0 is ignored.
        /// Bit 1-15 = 5x5 grid (binary), mirroed:
        /// Bits 16 - 31 encodes an RGB565 color
        /// The bits for the color are read in reverse since it's more convenient that way and the order is not relevant
        /// </summary>
        public uint ProfilePicture { get; } = TimesynqRandomizer.GenerateIdenticon();

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
        public ICollection<Follow> Followees { get; private set; } = [];

        /// <summary>
        /// Navigation property for all wips that the user has saved.
        /// </summary>
        public ICollection<Wip> Wips { get; private set; } = [];

        /// <summary>
        /// Navigation property for all wips that have been shared with the user.
        /// </summary>
        public ICollection<Share> SharedWips { get; private set; } = [];

        public Result ChangeUserName(string newUserName)
        {
            if (string.IsNullOrEmpty(newUserName) || newUserName.Length < UserConstants.MinUserNameLength || newUserName.Length > UserConstants.MaxUserNameLength)
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
