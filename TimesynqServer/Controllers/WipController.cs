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

        public WipController(IWipService wipServer)
        {
            _wipService = wipServer;
        }

        [HttpGet("{wipId}")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
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

        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<WipDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWips(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = PaginationConstants.DefaultPageSize,
            [FromQuery] string sortOrder = PaginationConstants.DefaultSortOrder,
            [FromQuery] string sortBy = PaginationConstants.DefaultWipSortBy
        )
        {
            return Ok(await _wipService.GetMyWipsAsync(CallerId, pageNumber, pageSize, sortOrder, sortBy, Request));
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
                onSuccess: Ok, // todo: use IHubContext to propagate the name change to hub users
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
        public async Task<IActionResult> DeleteWip([FromBody] DeleteWipRequestDTO deleteRequest)
        {
            Result deleteWipResult = await _wipService.DeleteWipAsync(CallerId, deleteRequest.WipID);
            return deleteWipResult.Match<IActionResult>
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
