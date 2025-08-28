namespace TimesynqServer.Contracts.Projections
{
    /// <summary>
    /// Represents only the TimesynqUser fields we need to retrieve from the database.
    /// </summary>
    public sealed class UserProjection
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The user's unique username.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// An 11 character string representing an identicon.
        /// The first 6 characters represent a hex color code.
        /// The 7th character is a period delimiter.
        /// The last 4 characters are a hex code encoding 15 bits of information. The most significant bit is always 0, so the highest possible value for the first hex digit is 7.
        /// These 15 bits are decoded into a 5x5 grid (binary), mirrored:
        /// Columns 1 and 2 are mapped to 5 and 4, respectively.
        /// In the grid, '1' represents the color (from the hex code), and '0' represents no color.
        /// </summary>
        public string ProfilePicture { get; }

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public DateTime CreatedOnUTC { get; }

        /// <summary>
        /// Constructs a <see cref="UserProjection"/> instance from TimesynqUser information. Used for filtering columns in queries.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="userName">The user's unique username.</param>
        /// <param name="profilePicture">A 21 character string representing an identicon.</param>
        /// <param name="createdOnUTC">The date when the user registered their account.</param>
        public UserProjection(Guid id, string userName, string profilePicture, DateTime createdOnUTC)
        {
            Id = id;
            UserName = userName;
            ProfilePicture = profilePicture;
            CreatedOnUTC = createdOnUTC;
        }

    }
}
