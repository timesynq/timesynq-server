namespace TimesynqServer.Models.DTO
{
    /// <summary>
    /// Represents only the publicly relevant information of a TimesynqUser.
    /// </summary>
    public sealed class UserDTO
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// The user's unique username.
        /// </summary>
        public required string UserName { get; set; }

        /// <summary>
        /// A 21 character string representing an identicon.
        /// First 6 chars = hex color;
        /// Next 15 = 5x5 grid (binary), mirrored:
        /// Cols 1=5, 2=4. '1' = color, '0' = white.
        /// </summary>
        public required string ProfilePicture { get; set; }

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public required DateTime CreatedOnUTC { get; set; }
    }
}
