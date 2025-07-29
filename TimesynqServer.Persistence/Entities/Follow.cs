using TimesynqServer.Common;
using TimesynqServer.Common.Result;

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
        public Guid FollowerId { get; private set; }

        /// <summary>
        /// The ID of the user who is being followed by another user.
        /// </summary>
        public Guid FolloweeId { get; private set; }

        /// <summary>
        /// The date when the follow action occurred.
        /// </summary>
        public DateTime CreatedOnUTC { get; private set; }

        /// <summary>
        /// Navigation property for the user who follows another user.
        /// </summary>
        public TimesynqUser? Follower { get; private set; }

        /// <summary>
        /// Navigation property for the user who is being followed by another user.
        /// </summary>
        public TimesynqUser? Followee { get; private set; }

        private Follow() { }

        private Follow(Guid followerId, Guid followeeId)
        {
            FollowerId = followerId;
            FolloweeId = followeeId;
            CreatedOnUTC = DateTime.UtcNow;
        }

        public static Result<Follow> Create(Guid followerId, Guid followeeId)
        {
            if(followerId == followeeId)
            {
                return Result<Follow>.Failure(DomainErrors.Follow.CantFollowYourself);
            }

            return Result<Follow>.Success(new Follow(followerId, followeeId));
        }

    }
}
