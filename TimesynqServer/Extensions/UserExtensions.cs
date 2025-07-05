using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Extensions
{
    public static class UserExtensions
    {
        public static UserDTO ToUserDTO(this TimesynqUser user)
        {
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName!,
                ProfilePicture = user.ProfilePicture,
                CreatedOnUTC = user.CreatedOnUTC,
            };
        }
    }
}
