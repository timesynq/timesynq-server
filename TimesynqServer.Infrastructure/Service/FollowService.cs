using Microsoft.AspNetCore.Http;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Domain.Entities.Follows;
using TimesynqServer.Contracts.Projections;
using TimesynqServer.Domain.Entities.Users;

namespace TimesynqServer.Infrastructure.Service.FollowService
{
    public class FollowService : IFollowService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;

        public FollowService(IUserRepository userRepository, IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }

        public async Task<Result<FollowDTO>> GetFollowAsync(Guid followerId, Guid followeeId)
        {
            FollowProjection? followProjection = await _followRepository.GetFollowAsync(followerId, followeeId);

            if (followProjection == null)
            {
                return Result<FollowDTO>.Failure(DomainErrors.Follow.NotFollowing);
            }

            return Result<FollowDTO>.Success(FollowDTO.FromProjection(followProjection));
        }

        public async Task<PagedResult<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalFollowers = await _followRepository.GetFollowersCountAsync(followeeId);

            int totalPages = (int)Math.Ceiling((double)totalFollowers / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followers = await _followRepository.GetFollowersAsync(followeeId, pageNumber, pageSize);

            IEnumerable<UserDTO> followerDTOs = followers.Select(UserDTO.FromProjection);

            return new PagedResult<UserDTO>(followerDTOs, pageNumber, pageSize, totalFollowers, totalPages, httpRequest);
        }

        public async Task<PagedResult<UserDTO>> GetFolloweesAsync(Guid followerId, int pageNumber, int pageSize, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, PaginationConstants.MinPageSize, PaginationConstants.MaxPageSize);

            int totalFollowees = await _followRepository.GetFolloweesCountAsync(followerId);

            int totalPages = (int)Math.Ceiling((double)totalFollowees / pageSize);

            if (totalPages <= 0)
            {
                return PagedResult<UserDTO>.CreateEmpty();
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followees = await _followRepository.GetFolloweesAsync(followerId, pageNumber, pageSize);

            IEnumerable<UserDTO> followeeDTOs = followees.Select(UserDTO.FromProjection);

            return new PagedResult<UserDTO>(followeeDTOs, pageNumber, pageSize, totalFollowees, totalPages, httpRequest);
        }

        public async Task<Result<FollowDTO>> FollowAsync(Guid followerId, Guid followeeId)
        {
            UserProjection? followerProjection = await _userRepository.GetByIdAsync(followerId);
            if (followerProjection == null)
            {
                return Result<FollowDTO>.Failure(DomainErrors.User.NotFound);
            }

            UserProjection? followeeProjection = await _userRepository.GetByIdAsync(followeeId);
            if (followeeProjection == null)
            {
                return Result<FollowDTO>.Failure(DomainErrors.User.NotFound);
            }

            FollowProjection? existingFollowProjection = await _followRepository.GetFollowAsync(followerId, followeeId);
            if (existingFollowProjection != null)
            {
                return Result<FollowDTO>.Failure(DomainErrors.Follow.AlreadyFollowing);
            }

            Result<Follow> followCreationResult = Follow.Create(followerId, followeeId);
            return await followCreationResult.Match
            (
                onSuccess: async follow =>
                {
                    await _followRepository.AddFollowAsync(follow);
                    return Result<FollowDTO>.Success(FollowDTO.FromFollow(follow));
                },
                onFailure: error => Task.FromResult(Result<FollowDTO>.Failure(error))
            );
        }

        public async Task<Result> UnfollowAsync(Guid followerId, Guid followeeId)
        {
            int deleted = await _followRepository.DeleteFollowAsync(followerId, followeeId);
            if (deleted == 0)
            {
                return Result.Failure(DomainErrors.Follow.NotFollowing);
            }
            return Result.Success();
        }

    }
}
