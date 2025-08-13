namespace TimesynqServer.DTO.Request.Follow
{
    /// <summary>
    /// Represents the required information for a follow request.
    /// </summary>
    public sealed class FollowRequestDTO
    {
        /// <summary>
        /// The ID of the user who is being followed.
        /// </summary>
        public required Guid FolloweeGuid { get; init; }
    }
}
