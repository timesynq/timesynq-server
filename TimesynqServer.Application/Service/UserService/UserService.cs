using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Common;
using TimesynqServer.Common.Extensions;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities;
using TimesynqServer.Persistence.Projections;
using TimesynqServer.Persistence.Repository.UserRepository;
using TimesynqServer.Persistence.UnitOfWork;

namespace TimesynqServer.Application.Service.UserService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
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

            if(searchString.Length < UserConstants.MinUserNameLength)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            searchString = searchString.Truncate(UserConstants.MaxUserNameLength);

            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

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

        public async Task<Result<UserDTO>> ChangeUserName(Guid userId, string newUserName)
        {
            TimesynqUser? user = await _userRepository.GetTrackedUserByIdAsync(userId);
            if (user == null)
            {
                return Result<UserDTO>.Failure(DomainErrors.User.NotFound);
            }

            bool isUserNameChangeCooldownOver = user.LastUpdatedUserNameUTC <= DateTime.UtcNow.AddDays(UserConstants.UserNameChangeCooldownDays * -1);

            if(!isUserNameChangeCooldownOver)
            {
                return Result<UserDTO>.Failure(DomainErrors.User.UserNameChangeOnCooldown);
            }

            UserProjection? userWithExistingUserName = await _userRepository.GetByUserNameAsync(newUserName);
            bool userNameIsTaken = userWithExistingUserName != null && userId != userWithExistingUserName.Id; 
            if (userNameIsTaken)
            {
                return Result<UserDTO>.Failure(DomainErrors.User.UserNameTaken);
            }

            Result changeUserNameResult = user.ChangeUserName(newUserName);
            return await changeUserNameResult.Match
            (
                onSuccess: async () =>
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Result<UserDTO>.Success(UserDTO.FromTimesynqUser(user));
                },
                onFailure: error => Task.FromResult(Result<UserDTO>.Failure(error))
            );
        }

        public async Task<Result> DeleteAccount(Guid userId)
        {
            TimesynqUser? user = await _userRepository.GetTrackedUserByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure(DomainErrors.User.NotFound);
            }

            Result softDeleteResult = user.SoftDelete();
            return await softDeleteResult.Match
            (
                onSuccess: async () =>
                {
                    await _unitOfWork.SaveChangesAsync();
                    return Result.Success();
                },
                onFailure: error => Task.FromResult(Result.Failure(error))
            );
        }

    }
}
