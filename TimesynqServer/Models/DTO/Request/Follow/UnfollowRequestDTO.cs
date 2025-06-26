namespace TimesynqServer.Models.DTO.Request.Follow
{
    public sealed class UnfollowRequestDTO
    {
        public required Guid FolloweeGuid { get; set; }
    }
}
