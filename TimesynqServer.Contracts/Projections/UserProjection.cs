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
        /// A 21 character string representing an identicon.
        /// First 6 chars = hex color;
        /// Next 15 = 5x5 grid (binary), mirrored:
        /// Cols 1=5, 2=4. '1' = color, '0' = white.
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
