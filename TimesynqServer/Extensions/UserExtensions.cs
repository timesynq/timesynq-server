using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Converts a <see cref="TimesynqUser"/> entity to a <see cref="UserDTO"/>.
        /// </summary>
        /// <param name="user">The <see cref="TimesynqUser"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="UserDTO"/> object containing the mapped values from the specified <see cref="TimesynqUser"/>.
        /// </returns>
        /// <remarks>
        /// This extension method maps the following properties:
        /// <list type="bullet">
        /// <item><description><c>Id</c> → <c>UserDTO.Id</c></description></item>
        /// <item><description><c>UserName</c> → <c>UserDTO.UserName</c></description></item>
        /// <item><description><c>ProfilePicture</c> → <c>UserDTO.ProfilePicture</c></description></item>
        /// <item><description><c>CreatedOnUTC</c> → <c>UserDTO.CreatedOnUTC</c></description></item>
        /// </list>
        /// </remarks>
        public static UserDTO ToUserDTO(this TimesynqUser user)
        {
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName!,
                ProfilePicture = user.ProfilePicture,
                CreatedOnUTC = user.CreatedOnUTC,
            };
        }
    }
}
