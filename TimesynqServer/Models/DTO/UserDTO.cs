using TimesynqServer.Database.Projections;

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
        public string ProfilePicture { get; set; }

        /// <summary>
        /// The date when the user registered their account.
        /// </summary>
        public DateTime CreatedOnUTC { get; set; }

        private UserDTO(Guid id, string userName, string profilePicture, DateTime createdOnUTC)
        {
            Id = id;
            UserName = userName;
            ProfilePicture = profilePicture;
            CreatedOnUTC = createdOnUTC;
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

            return new UserDTO(projection.Id, projection.UserName, projection.ProfilePicture, projection.CreatedOnUTC);
        }

    }
}
