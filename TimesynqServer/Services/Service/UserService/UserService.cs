using TimesynqServer.Database.Projections;
using TimesynqServer.Database.Repository.UserRepository;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Services.Service.UserService
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserDTO?> GetUserAsync(Guid userId)
        {
            UserProjection? userProjection = await _userRepository.GetByIdAsync(userId);
            if (userProjection == null)
            {
                return null;
            }

            return UserDTO.FromProjection(userProjection);
        }
    }
}
