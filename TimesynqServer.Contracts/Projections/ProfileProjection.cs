namespace TimesynqServer.Contracts.Projections
{
    public class ProfileProjection
    {
        public UserProjection UserProjection { get; }
        public bool IsFollowing { get; }

        public ProfileProjection(UserProjection userProjection, bool isFollowing)
        {
            UserProjection = userProjection;
            IsFollowing = isFollowing;
        }

    }
}
