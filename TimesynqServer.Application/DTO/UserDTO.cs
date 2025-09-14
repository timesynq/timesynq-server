using TimesynqServer.Domain.Entities.Users;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents only the publicly relevant information of a TimesynqUser.
    /// </summary>
    public sealed class UserDTO
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

        private UserDTO(Guid id, string userName, uint profilePicture, DateTime createdOnUTC, int followerCount, int followeeCount)
        {
            Id = id;
            UserName = userName;
            ProfilePicture = profilePicture;
            CreatedOnUTC = createdOnUTC;
            FollowerCount = followerCount;
            FolloweeCount = followeeCount;
        }

        /// <summary>
        /// Converts a <see cref="UserProjection"/> to a <see cref="UserDTO"/>.
        /// </summary>
        /// <param name="projection">The <see cref="UserProjection"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="UserDTO"/> object containing the mapped values from the specified <see cref="UserProjection"/>.
        /// </returns>
        /// <remarks>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="projection"/> is null.
        /// This method is used for standardized instantiation of <see cref="UserDTO"/>.
        /// </remarks>
        public static UserDTO FromProjection(UserProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new UserDTO(projection.Id, projection.UserName, projection.ProfilePicture, projection.CreatedOnUTC, projection.FollowerCount, projection.FolloweeCount);
        }

        /// <summary>
        /// Converts a <see cref="TimesynqUser"/> to a <see cref="UserDTO"/>.
        /// </summary>
        /// <param name="timesynqUser">The <see cref="TimesynqUser"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="UserDTO"/> object containing the mapped values from the specified <see cref="TimesynqUser"/>.
        /// </returns>
        /// <remarks>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="timesynqUser"/> is null.
        /// This method is used for standardized instantiation of <see cref="UserDTO"/>.
        /// </remarks>
        public static UserDTO FromTimesynqUser(TimesynqUser timesynqUser)
        {
            ArgumentNullException.ThrowIfNull(timesynqUser);

            return new UserDTO(timesynqUser.Id, timesynqUser.UserName!, timesynqUser.ProfilePicture, timesynqUser.CreatedOnUTC, timesynqUser.Followers.Count, timesynqUser.Followees.Count);
        }

    }
}
