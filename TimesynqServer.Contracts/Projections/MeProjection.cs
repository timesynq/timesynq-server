namespace TimesynqServer.Contracts.Projections
{
    public class MeProjection
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
        /// An unsigned 32 bit integer, which encodes an identicon
        /// Bit 0 is ignored.
        /// Bit 1-15 = 5x5 grid (binary), mirroed:
        /// Bits 16 - 31 encodes an RGB565 color
        /// The bits for the color are read in reverse since it's more convenient that way and the order is not relevant
        /// </summary>
        public uint ProfilePicture { get; }

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public DateTime CreatedOnUTC { get; }

        /// <summary>
        /// The number of people following this user.
        /// </summary>
        public int FollowerCount { get; }

        /// <summary>
        /// The number of people this user is following.
        /// </summary>
        public int FolloweeCount { get; }

        /// <summary>
        /// The email used to register this user's account.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Boolean for if the user has confirmed their email or not.
        /// </summary>
        public bool EmailConfirmed { get; }

        /// <summary>
        /// Constructs a <see cref="MeProjection"/> instance from TimesynqUser information. Used for filtering columns in queries.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="userName">The user's unique username.</param>
        /// <param name="profilePicture">An unsigned 32 bit integer, which encodes an identicon.</param>
        /// <param name="createdOnUTC">The date when the user registered their account.</param>
        /// <param name="followerCount">The number of people following this user.</param>
        /// <param name="followeeCount">The number of people this user is following.</param>
        /// <param name="email">The email used to register this user's account.</param>
        /// <param name="emailConfirmed">Boolean for if the user has confirmed their email or not.</param>
        public MeProjection(Guid id, string userName, uint profilePicture, DateTime createdOnUTC, int followerCount, int followeeCount, string email, bool emailConfirmed)
        {
            Id = id;
            UserName = userName;
            ProfilePicture = profilePicture;
            CreatedOnUTC = createdOnUTC;
            FollowerCount = followerCount;
            FolloweeCount = followeeCount;
            Email = email;
            EmailConfirmed = emailConfirmed;
        }
    }
}
