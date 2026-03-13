using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    public class SharedUserDTO : UserDTO
    {
        public bool IsAccepted { get; }

        private SharedUserDTO(
            Guid id,
            string userName,
            uint profilePicture, 
            DateTime createdOnUTC, 
            int followerCount,
            int followeeCount,
            bool isAccepted
        ) : base(
            id,
            userName,
            profilePicture,
            createdOnUTC,
            followerCount,
            followeeCount
        )
        {
            IsAccepted = isAccepted;
        }

        public static SharedUserDTO FromProjection(SharedUserProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new SharedUserDTO(
                projection.Id,
                projection.UserName,
                projection.ProfilePicture,
                projection.CreatedOnUTC,
                projection.FollowerCount,
                projection.FolloweeCount,
                projection.IsAccepted
            );
        }
    }
}
