using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service.FollowService;
using TimesynqServer.Common.Result;
using TimesynqServer.DTO.Request.Follow;
using TimesynqServer.Application.Pagination;

namespace TimesynqServer.Controllers
{
    [Route("follows")]
    [ApiController]
    public class FollowController : AuthorizedController
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpGet("{followeeGuid}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(FollowDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollow(Guid followeeGuid)
        {
            Result<FollowDTO> getFollowResult = await _followService.GetFollowAsync(CallerGuid, followeeGuid);
            return getFollowResult.Match
            (
                onSuccess: Ok,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequestDTO followRequest)
        {
            Result<FollowDTO> followResult = await _followService.FollowAsync(CallerGuid, followRequest.FolloweeGuid);
            return followResult.Match
            (
                onSuccess: followDTO =>
                {
                    string resourceUri = $"{Request.Scheme}://{Request.Host}{Request.Path}{followRequest.FolloweeGuid}";
                    return Created(resourceUri, followDTO);
                },
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpDelete]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnfollowUser([FromBody] UnfollowRequestDTO unfollowRequest)
        {
            Result unfollowResult = await _followService.UnfollowAsync(CallerGuid, unfollowRequest.FolloweeGuid);
            return unfollowResult.Match<IActionResult>
            (
                onSuccess: NoContent,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

    }
}
