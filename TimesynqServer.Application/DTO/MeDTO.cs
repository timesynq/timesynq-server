using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents the TimesynqUser information a client needs to know about itself.
    /// </summary>
    public class MeDTO
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

        private MeDTO(Guid id, string userName, uint profilePicture, DateTime createdOnUTC, int followerCount, int followeeCount, string email, bool emailConfirmed)
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

        /// <summary>
        /// Converts a <see cref="MeProjection"/> to a <see cref="MeDTO"/>.
        /// </summary>
        /// <param name="projection">The <see cref="MeProjection"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="MeDTO"/> object containing the mapped values from the specified <see cref="MeProjection"/>.
        /// </returns>
        /// <remarks>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="projection"/> is null.
        /// This method is used for standardized instantiation of <see cref="MeDTO"/>.
        /// </remarks>
        public static MeDTO FromProjection(MeProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new MeDTO(projection.Id, projection.UserName, projection.ProfilePicture, projection.CreatedOnUTC, projection.FollowerCount, projection.FolloweeCount, projection.Email, projection.EmailConfirmed);
        }
    }
}
