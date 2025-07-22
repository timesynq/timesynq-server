namespace TimesynqServer.Database.Entities
{
    /// <summary>
    /// Represents a follower-following relationship between two users.
    /// </summary>
    public sealed class Follow
    {
        /// <summary>
        /// The ID of the user who follows another user.
        /// </summary>
        public Guid FollowerId { get; set; }

        /// <summary>
        /// The ID of the user who is being followed by another user.
        /// </summary>
        public Guid FolloweeId { get; set; }

        /// <summary>
        /// The date when the follow action occurred.
        /// </summary>
        public DateTime CreatedOnUTC { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Navigation property for the user who follows another user.
        /// </summary>
        public TimesynqUser? Follower { get; set; }

        /// <summary>
        /// Navigation property for the user who is being followed by another user.
        /// </summary>
        public TimesynqUser? Followee { get; set; }
    }
}
