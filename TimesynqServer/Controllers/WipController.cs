using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.RequestDTO.Wip;

namespace TimesynqServer.Controllers
{
    [Route("wips")]
    [ApiController]
    public class WipController : AuthorizedController
    {
        private readonly IWipService _wipService;
        private readonly IShareService _shareService;

        public WipController(IWipService wipService, IShareService shareService)
        {
            _wipService = wipService;
            _shareService = shareService;
        }

        [HttpGet("{wipId}")]
        [ProducesResponseType(typeof(WipDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWip(Guid wipId)
        {
            WipDTO? wipDTO = await _wipService.GetWipAsync(CallerId, wipId);
            if(wipDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: DomainErrors.Wip.NotFound.Message
                );
            }

            return Ok(wipDTO);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<WipDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWips(
            [FromQuery] string? searchString,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
            [FromQuery] string sortOrder = PaginationConstants.DefaultSortOrder,
            [FromQuery] string sortBy = PaginationConstants.DefaultWipSortBy
        )
        {
            return Ok(await _wipService.GetMyWipsAsync(CallerId, searchString, pageNumber, pageSize, sortOrder, sortBy, Request));
        }

        [HttpPost]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(WipDTO), StatusCodes.Status201Created)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateWip()
        {
            Result<WipDTO> createResult = await _wipService.CreateWipAsync(CallerId);
            return createResult.Match
            (
                onSuccess: wipDTO =>
                {
                    string resourceUri = $"{Request.Scheme}://{Request.Host}{Request.Path}{wipDTO.Id}";
                    return Created(resourceUri, wipDTO);
                },
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpPatch("{wipId}/name")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(WipDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangeWipName(Guid wipId, [FromBody] ChangeWipNameRequestDTO changeWipNameRequest)
        {
            Result<WipDTO> changeWipNameResult = await _wipService.ChangeWipName(CallerId, wipId, changeWipNameRequest.NewName);
            return changeWipNameResult.Match<IActionResult>
            (
                onSuccess: Ok,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpDelete("{wipId}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWip(Guid wipId)
        {
            Result deleteWipResult = await _wipService.DeleteWipAsync(CallerId, wipId);
            return deleteWipResult.Match<IActionResult>
            (
                onSuccess: NoContent,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpGet("{wipId}/shares")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(IEnumerable<SharedUserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSharedUsers(Guid wipId)
        {
            return Ok(await _shareService.GetSharedUsersAsync(CallerId, wipId));
        }

        [HttpGet("shared")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<SharedWipDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSharedWips(
            [FromQuery] string? searchString,
            [FromQuery] bool isAccepted = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
            [FromQuery] string sortOrder = PaginationConstants.DefaultSortOrder,
            [FromQuery] string sortBy = PaginationConstants.DefaultShareSortBy
        )
        {
            return Ok(await _shareService.GetSharedWipsAsync(CallerId, isAccepted, searchString, pageNumber, pageSize, sortOrder, sortBy, Request));
        }

        [HttpPost("{wipId}/shares")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status201Created)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ShareWip(Guid wipId, [FromBody] ShareWipRequestDTO shareWipRequestDTO)
        {
            Result<UserDTO> shareResult = await _shareService.ShareWipAsync(CallerId, wipId, shareWipRequestDTO.ShareWithId);
            return shareResult.Match
            (
                onSuccess: userDTO =>
                {
                    return Created((string?)null, userDTO);
                },
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpPatch("{wipId}/shares")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> AcceptShare(Guid wipId) 
        {
            Result acceptResult = await _shareService.AcceptShareAsync(CallerId, wipId);
            return acceptResult.Match<IActionResult>
            (
                onSuccess: NoContent,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpDelete("{wipId}/shares/{userId}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnshareOneFromWip(Guid wipId, Guid userId)
        {
            Result unshareResult = await _shareService.UnshareFromWipAsync(CallerId, wipId, userId);
            return unshareResult.Match<IActionResult>
            (
                onSuccess: NoContent,
                onFailure: error => Problem(
                    statusCode: error.Code,
                    detail: error.Message
                )
            );
        }

        [HttpDelete("{wipId}/shares")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnshareAllFromWip(Guid wipId)
        {
            Result unshareResult = await _shareService.UnshareAllFromWipAsync(CallerId, wipId);
            return unshareResult.Match<IActionResult>
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
