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
using TimesynqServer.Persistence.Projections;

namespace TimesynqServer.Controllers
{
    [Route("follows")]
    [ApiController]
    public class FollowController : ControllerBase
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
        [ProducesResponseType(typeof(FollowDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollow(Guid followeeGuid)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: "Invalid token."
                );
            }
            Guid callerGuid = Guid.Parse(callerId);

            FollowProjection? followProjection = await _followRepository.GetFollowAsync(callerGuid, followeeGuid);

            if (followProjection == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "Not following this user."
                );
            }

            return Ok(FollowDTO.FromProjection(followProjection));
        }

        [HttpGet("{userId}/followers")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowers = await _followRepository.GetFollowersCountAsync(userId);

            int totalPages = (int)Math.Ceiling((double)totalFollowers / pageSize);

            if (totalPages <= 0)
            {
                return Ok(new List<UserDTO>());
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followers = await _followRepository.GetFollowersAsync(userId, pageNumber, pageSize);

            IEnumerable<UserDTO> followerDTOs = followers.Select(UserDTO.FromProjection);

            var pagedResult = new PagedResult<UserDTO>(followerDTOs, pageNumber, pageSize, totalFollowers, totalPages, Request);

            return Ok(pagedResult);
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowees(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowees = await _followRepository.GetFolloweesCountAsync(userId);

            int totalPages = (int)Math.Ceiling((double)totalFollowees / pageSize);

            if (totalPages <= 0)
            {
                return Ok(new List<UserDTO>());
            }

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserProjection> followees = await _followRepository.GetFolloweesAsync(userId, pageNumber, pageSize);

            IEnumerable<UserDTO> followeeDTOs = followees.Select(UserDTO.FromProjection);

            var pagedResult = new PagedResult<UserDTO>(followeeDTOs, pageNumber, pageSize, totalFollowees, totalPages, Request);

            return Ok(pagedResult);
        }

        [HttpPost]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(FollowDTO), StatusCodes.Status201Created)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequestDTO followRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: "Invalid token."
                );
            }
            Guid callerGuid = Guid.Parse(callerId);

            if (callerGuid == followRequest.FolloweeGuid)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: "Can't follow yourself."
                );
            }

            UserProjection? followeeProjection = await _userRepository.GetByIdAsync(followRequest.FolloweeGuid);
            if (followeeProjection == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "Followee doesn't exist."
                );
            }

            FollowProjection? existingFollowProjection = await _followRepository.GetFollowAsync(callerGuid, followRequest.FolloweeGuid);

            if (existingFollowProjection != null)
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    detail: "Already following this user."
                );
            }

            FollowProjection persistedFollowProjection = await _followRepository.FollowAsync(callerGuid, followRequest.FolloweeGuid);
            string resourceUri = $"{Request.Scheme}://{Request.Host}{Request.Path}{followRequest.FolloweeGuid}";

            return Created(resourceUri, FollowDTO.FromProjection(persistedFollowProjection));
        }

        [HttpDelete]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnfollowUser([FromBody] UnfollowRequestDTO unfollowRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    detail: "Invalid token."
                );
            }
            Guid callerGuid = Guid.Parse(callerId);

            FollowProjection? existingFollowProjection = await _followRepository.GetFollowAsync(callerGuid, unfollowRequest.FolloweeGuid);

            if (existingFollowProjection == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "Not following this user."
                );
            }

            await _followRepository.UnfollowAsync(existingFollowProjection.FollowerId, existingFollowProjection.FolloweeId);

            return NoContent();
        }

    }
}
