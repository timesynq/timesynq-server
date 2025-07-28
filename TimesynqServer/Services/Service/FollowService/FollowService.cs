using Azure.Core;
using TimesynqServer.Database.Projections;
using TimesynqServer.Database.Repository.FollowRepository;
using TimesynqServer.Models.DTO;
using TimesynqServer.Models.Pagination;
using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Services.Service.FollowService
{
    public class FollowService : IFollowService
    {

        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository) 
        {
            _followRepository = followRepository;
        }

        public async Task<FollowDTO?> GetFollowAsync(Guid followerId, Guid followeeId)
        {
            FollowProjection? followProjection = await _followRepository.GetFollowAsync(followerId, followeeId);

            if (followProjection == null)
            {
                return null;
            }

            return FollowDTO.FromProjection(followProjection);
        }

        public async Task<PagedResult<UserDTO>> GetFollowersAsync(Guid followeeId, int pageNumber, int pageSize, HttpRequest httpRequest)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

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
            pageSize = Math.Clamp(pageSize, 1, 100);

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

    }
}
