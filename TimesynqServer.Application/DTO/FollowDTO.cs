using TimesynqServer.Domain.Entities.Follows;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    /// <summary>
    /// Represents the publicly relevant information of a follower-following relationship between two users.
    /// </summary>
    public sealed class FollowDTO
    {
        /// <summary>
        /// The ID of the user who follows another user.
        /// </summary>
        public Guid FollowerId { get; }

        /// <summary>
        /// The ID of the user who is being followed by another user.
        /// </summary>
        public Guid FolloweeId { get; }

        /// <summary>
        /// The date when the follow action occurred.
        /// </summary>
        public DateTime CreatedOnUTC { get; }

        private FollowDTO(Guid followerId, Guid followeeId, DateTime createdOnUTC)
        {
            FollowerId = followerId;
            FolloweeId = followeeId;
            CreatedOnUTC = createdOnUTC;
        }

        /// <summary>
        /// Converts a <see cref="FollowProjection"/> to a <see cref="FollowDTO"/>.
        /// </summary>
        /// <param name="projection">The <see cref="FollowProjection"/> instance to convert.</param>
        /// <returns>
        /// A <see cref="FollowDTO"/> object containing the mapped values from the specified <see cref="FollowProjection"/>.
        /// </returns>
        /// <remarks>
        /// Throws an <see cref="ArgumentNullException"/> if <paramref name="projection"/> is null.
        /// This method is used for standardized instantiation of <see cref="FollowDTO"/>.
        /// </remarks>
        public static FollowDTO FromProjection(FollowProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new FollowDTO(projection.FollowerId, projection.FolloweeId, projection.CreatedOnUTC);
        }

        public static FollowDTO FromFollow(Follow follow)
        {
            ArgumentNullException.ThrowIfNull(follow);

            return new FollowDTO(follow.FollowerId, follow.FolloweeId, follow.CreatedOnUTC);
        }

    }
}
