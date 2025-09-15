using System.Text.Json.Serialization;
using TimesynqServer.Contracts.Projections;

namespace TimesynqServer.Application.DTO
{
    public class ProfileDTO
    {
        [JsonPropertyName("user")]
        public UserDTO UserDTO { get; }

        public bool IsFollowing { get; }

        public ProfileDTO(UserDTO userDTO, bool isFollowing)
        {
            UserDTO = userDTO;
            IsFollowing = isFollowing;
        }

        public static ProfileDTO FromProjection(ProfileProjection projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            return new ProfileDTO(UserDTO.FromProjection(projection.UserProjection), projection.IsFollowing);
        }
    }
}
