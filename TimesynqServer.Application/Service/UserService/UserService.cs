using TimesynqServer.Application.DTO;
using TimesynqServer.Persistence.Projections;
using TimesynqServer.Persistence.Repository.UserRepository;

namespace TimesynqServer.Application.Service.UserService
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
