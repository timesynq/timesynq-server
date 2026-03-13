namespace TimesynqServer.Contracts.Projections
{
    public sealed class SharedUserProjection : UserProjection
    {
        public bool IsAccepted { get; }

        public SharedUserProjection(
            Guid id,
            string userName, 
            uint profilePicture, 
            DateTime createdOnUTC, 
            int followerCount,
            int followeeCount, 
            bool isAccepted
        )
            : base(id, userName, profilePicture, createdOnUTC, followerCount, followeeCount)
        {
            IsAccepted = isAccepted;
        }
    }
}
