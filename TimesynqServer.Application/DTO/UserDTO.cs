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

            return new UserDTO(timesynqUser.Id, timesynqUser.UserName!, timesynqUser.ProfilePicture, timesynqUser.CreatedOnUTC);
        }

    }
}
