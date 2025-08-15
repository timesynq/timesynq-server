using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common.Extensions;
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

        public async Task<bool> IsUserConfirmed(Guid userId)
        {
            return await _userRepository.GetConfirmedUserByIdAsync(userId);
        }

        public async Task<PagedResult<UserDTO>> SearchUsers(string searchString, int pageNumber, int pageSize, HttpRequest httpRequest)
        {

            if(searchString.Length < 3)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            searchString = searchString.Truncate(24);

            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalUsersContainingSearchString = await _userRepository.GetTotalUsersContainingSearchStringAsync(searchString);

            int totalPages = (int)Math.Ceiling((double)totalUsersContainingSearchString / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> users = await _userRepository.GetUsersContainingSearchStringAsync(searchString, pageNumber, pageSize);

            IEnumerable<UserDTO> userDTOs = users.Select(UserDTO.FromProjection);

            return new PagedResult<UserDTO>(userDTOs, pageNumber, pageSize, totalUsersContainingSearchString, totalPages, httpRequest);
        }
    }
}
