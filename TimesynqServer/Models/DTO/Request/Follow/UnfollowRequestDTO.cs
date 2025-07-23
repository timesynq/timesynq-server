namespace TimesynqServer.Models.DTO.Request.Follow
{
    /// <summary>
    /// Represents the required information for an unfollow request.
    /// </summary>
    public sealed class UnfollowRequestDTO
    {
        /// <summary>
        /// The ID of the user who is being followed.
        /// </summary>
        public required Guid FolloweeGuid { get; init; }
    }
}
