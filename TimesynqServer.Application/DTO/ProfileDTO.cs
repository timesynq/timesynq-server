using System.Text.Json.Serialization;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    public class ProfileDTO
    {
        [JsonPropertyName("user")]
        public UserProjection UserProjection { get; }

        public bool IsFollowing { get; }

        public ProfileDTO(UserProjection userProjection, bool isFollowing)
        {
            UserProjection = userProjection;
            IsFollowing = isFollowing;
        }

        public static ProfileDTO FromProjection(ProfileProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new ProfileDTO(projection.UserProjection, projection.IsFollowing);
        }
    }
}
