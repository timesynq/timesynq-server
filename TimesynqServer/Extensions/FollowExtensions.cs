using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Extensions
{
    public static class FollowExtensions
    {
        /// <summary>
        /// Converts a <see cref="Follow"/> entity to a <see cref="FollowDTO"/>.
        /// </summary>
        /// <param name="follow">The <see cref="Follow"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="FollowDTO"/> object containing the mapped values from the specified <see cref="Follow"/>.
        /// </returns>
        /// <remarks>
        /// This extension method maps the following properties:
        /// <list type="bullet">
        /// <item><description><c>FollowerId</c> → <c>FollowDTO.FollowerId</c></description></item>
        /// <item><description><c>FolloweeId</c> → <c>FollowDTO.FolloweeId</c></description></item>
        /// <item><description><c>CreatedOnUTC</c> → <c>FollowDTO.CreatedOnUTC</c></description></item>
        /// </list>
        /// </remarks>
        public static FollowDTO ToFollowDTO(this Follow follow)
        {
            return new FollowDTO
            {
                FollowerId = follow.FollowerId,
                FolloweeId = follow.FolloweeId,
                CreatedOnUTC = follow.CreatedOnUTC,
            };
        }
    }
}
