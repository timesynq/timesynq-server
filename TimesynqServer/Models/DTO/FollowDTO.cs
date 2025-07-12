namespace TimesynqServer.Models.DTO
{
    /// <summary>
    /// Represents the publicly relevant information of a follower-following relationship between two users.
    /// </summary>
    public sealed class FollowDTO
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
    }
}
