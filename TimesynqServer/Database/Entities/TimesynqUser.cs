using Microsoft.AspNetCore.Identity;

namespace TimesynqServer.Database.Entities
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
        /// A 21 character string representing an identicon.
        /// First 6 chars = hex color;
        /// Next 15 = 5x5 grid (binary), mirrored:
        /// Cols 1=5, 2=4. '1' = color, '0' = white.
        /// </summary>
        public string ProfilePicture { get; set; } = string.Empty;

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public DateTime CreatedOnUTC { get; set; }

        /// <summary>
        /// The date when the user's information was last changed.
        /// </summary>
        public DateTime LastUpdatedOnUTC { get; set; }

        /// <summary>
        /// Navigation property for all users that are following this user.
        /// </summary>
        public ICollection<Follow> Followers { get; set; } = [];

        /// <summary>
        /// Navigation property for all users that this user is following.
        /// </summary>
        public ICollection<Follow> Following { get; set; } = [];


    }
}
