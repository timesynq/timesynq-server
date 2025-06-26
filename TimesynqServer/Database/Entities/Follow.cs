namespace TimesynqServer.Database.Entities
{
    public sealed class Follow
    {
        public Guid FollowerId { get; set; }
        public Guid FolloweeId { get; set; }
        public DateTime CreatedOnUTC { get; set; } = DateTime.UtcNow;

        //Navigation Properties
        public TimesynqUser? Follower { get; set; }
        public TimesynqUser? Followee { get; set; }
    }
}
