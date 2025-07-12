using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;
using TimesynqServer.Models.DTO.Request.Follow;
using TimesynqServer.Models.Pagination;
using TimesynqServer.Services.Repository.FollowRepository;
using TimesynqServer.Services.Repository.UserRepository;

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

            IEnumerable<UserDTO> followers = await _followRepository.GetFollowersAsync(userId, pageNumber, pageSize);

            PagedResult<UserDTO> pagedResult = followers.ToPagedResult(pageNumber, pageSize, totalFollowers, totalPages, Request);

            return OkResponse(StatusCodes.Status200OK, pagedResult);
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
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

            IEnumerable<UserDTO> followees = await _followRepository.GetFolloweesAsync(userId, pageNumber, pageSize);

            PagedResult<UserDTO> pagedResult = followees.ToPagedResult(pageNumber, pageSize, totalFollowees, totalPages, Request);

            return OkResponse(StatusCodes.Status200OK, pagedResult);
        }

        [HttpPost]
        [Authorize(Roles = "ConfirmedUser, Admin")]
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

            TimesynqUser? followee = await _userRepository.GetById(followRequest.FolloweeGuid);
            if (followee == null)
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
