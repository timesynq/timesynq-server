using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;
using TimesynqServer.Database.Repository.FollowRepository;
using TimesynqServer.Database.Repository.UserRepository;
using TimesynqServer.Extensions;
using TimesynqServer.Migrations;
using TimesynqServer.Models.DTO;
using TimesynqServer.Models.DTO.Request.Follow;
using TimesynqServer.Models.Pagination;

namespace TimesynqServer.Controllers
{
    [Route("follows")]
    [ApiController]
    public class FollowController : TimesynqController
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;

        public FollowController(IUserRepository userRepository, IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
        }

        [HttpGet("{followeeGuid}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(ResponseDTO<FollowDTO>), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ResponseDTO<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollow(Guid followeeGuid)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return ErrorResponse(StatusCodes.Status401Unauthorized, "Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            Follow? follow = await _followRepository.GetFollowAsync(callerGuid, followeeGuid);

            if (follow == null)
            {
                return ErrorResponse(StatusCodes.Status404NotFound, "Not following this user");
            }

            return OkResponse(StatusCodes.Status200OK, follow.ToFollowDTO());
        }

        [HttpGet("{userId}/followers")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO<PagedResult<UserDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowers = await _followRepository.GetFollowersCountAsync(userId);

            int totalPages = (int)Math.Ceiling((double)totalFollowers / pageSize);

            if (totalPages <= 0)
            {
                return OkResponse(StatusCodes.Status200OK, new List<UserDTO>());
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followers = await _followRepository.GetFollowersAsync(userId, pageNumber, pageSize);

            IEnumerable<UserDTO> followerDTOs = followers.Select(UserDTO.FromProjection);

            PagedResult<UserDTO> pagedResult = followerDTOs.ToPagedResult(pageNumber, pageSize, totalFollowers, totalPages, Request);

            return OkResponse(StatusCodes.Status200OK, pagedResult);
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO<PagedResult<UserDTO>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowees(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowees = await _followRepository.GetFolloweesCountAsync(userId);

            int totalPages = (int)Math.Ceiling((double)totalFollowees / pageSize);

            if (totalPages <= 0)
            {
                return OkResponse(StatusCodes.Status200OK, new List<UserDTO>());
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followees = await _followRepository.GetFolloweesAsync(userId, pageNumber, pageSize);

            IEnumerable<UserDTO> followeeDTOs = followees.Select(UserDTO.FromProjection);

            PagedResult<UserDTO> pagedResult = followeeDTOs.ToPagedResult(pageNumber, pageSize, totalFollowees, totalPages, Request);

            return OkResponse(StatusCodes.Status200OK, pagedResult);
        }

        [HttpPost]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(FollowDTO), StatusCodes.Status201Created)]
        [ProducesErrorResponseType(typeof(ResponseDTO<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequestDTO followRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return ErrorResponse(StatusCodes.Status401Unauthorized, "Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            if (callerGuid == followRequest.FolloweeGuid)
            {
                return ErrorResponse(StatusCodes.Status409Conflict, "Can't follow yourself");
            }

            UserProjection? followeeProjection = await _userRepository.GetByIdAsync(followRequest.FolloweeGuid);
            if (followeeProjection == null)
            {
                return ErrorResponse(StatusCodes.Status404NotFound, "Followee doesn't exist");
            }

            Follow? existingFollow = await _followRepository.GetFollowAsync(callerGuid, followRequest.FolloweeGuid);

            if (existingFollow != null)
            {
                return ErrorResponse(StatusCodes.Status409Conflict, "Already following this user");
            }

            Follow persistedFollow = await _followRepository.FollowAsync(callerGuid, followRequest.FolloweeGuid);
            string resourceUri = $"{Request.Scheme}://{Request.Host}{Request.Path}{followRequest.FolloweeGuid}";

            return OkResponse(StatusCodes.Status201Created, persistedFollow.ToFollowDTO(), resourceUri);
        }

        [HttpDelete]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ResponseDTO<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnfollowUser([FromBody] UnfollowRequestDTO unfollowRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return ErrorResponse(StatusCodes.Status401Unauthorized, "Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            Follow? existingFollow = await _followRepository.GetFollowAsync(callerGuid, unfollowRequest.FolloweeGuid);

            if (existingFollow == null)
            {
                return ErrorResponse(StatusCodes.Status404NotFound, "Not following this user");
            }

            await _followRepository.UnfollowAsync(existingFollow);

            return NoContent();
        }

    }
}
