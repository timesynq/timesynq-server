namespace TimesynqServer.Contracts.Projections
{
    /// <summary>
    /// Represents only the Follow fields we need to retrieve from the database.
    /// </summary>
    public class FollowProjection
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

        /// <summary>
        /// Constructs a <see cref="FollowProjection"/> instance from Follower information. Used for filtering columns in queries.
        /// </summary>
        /// <param name="followerId">The unique identifier of the user who is following another user.</param>
        /// <param name="followeeId">The unique identifier of the user who is being followed by another user.</param>
        /// <param name="createdOnUTC">The date when the following occurred.</param>
        public FollowProjection(Guid followerId, Guid followeeId, DateTime createdOnUTC)
        {
            FollowerId = followerId;
            FolloweeId = followeeId;
            CreatedOnUTC = createdOnUTC;
        }
    }
}
