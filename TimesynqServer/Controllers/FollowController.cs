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
using TimesynqServer.Services.Service.FollowService;

namespace TimesynqServer.Controllers
{
    [Route("follows")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IFollowRepository _followRepository;
        private readonly IFollowService _followService;

        public FollowController(IUserRepository userRepository, IFollowRepository followRepository, IFollowService followService)
        {
            _userRepository = userRepository;
            _followRepository = followRepository;
            _followService = followService;
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

            FollowDTO? followDTO = await _followService.GetFollowAsync(callerGuid, followeeGuid);

            if (followDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "Not following this user."
                );
            }

            return Ok(followDTO);
        }

        [HttpGet("{userId}/followers")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            return Ok(await _followService.GetFollowersAsync(userId, pageNumber, pageSize, Request));
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowees(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            return Ok(await _followService.GetFolloweesAsync(userId, pageNumber, pageSize, Request));
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

            FollowProjection persistedFollowProjection = await _followRepository.AddFollowAsync(callerGuid, followRequest.FolloweeGuid);
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

            await _followRepository.DeleteFollowAsync(existingFollowProjection.FollowerId, existingFollowProjection.FolloweeId);

            return NoContent();
        }

    }
}
