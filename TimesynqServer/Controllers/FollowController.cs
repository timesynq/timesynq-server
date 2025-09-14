using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.RequestDTO.Follow;

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

        [HttpGet("{followeeId}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(FollowDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFollow(Guid followeeId)
        {
            Result<FollowDTO> getFollowResult = await _followService.GetFollowAsync(CallerId, followeeId);
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
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = PaginationConstants.DefaultPageSize)
        {
            return Ok(await _followService.GetFollowersAsync(userId, pageNumber, pageSize, Request));
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFollowees(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = PaginationConstants.DefaultPageSize)
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
            Result<FollowDTO> followResult = await _followService.FollowAsync(CallerId, followRequest.FolloweeId);
            return followResult.Match
            (
                onSuccess: followDTO =>
                {
                    string resourceUri = $"{Request.Scheme}://{Request.Host}{Request.Path}{followRequest.FolloweeId}";
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
            Result unfollowResult = await _followService.UnfollowAsync(CallerId, unfollowRequest.FolloweeId);
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
