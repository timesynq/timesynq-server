using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Pagination;
using TimesynqServer.Application.Service;
using TimesynqServer.Common;
using TimesynqServer.Common.Result;
using TimesynqServer.Contracts.RequestDTO.User;

namespace TimesynqServer.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : AuthorizedController
    {

        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me()
        {
            UserDTO? userDTO = await _userService.GetUserAsync(CallerId);
            if (userDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: DomainErrors.User.NotFound.Message
                );
            }

            return Ok(userDTO);
        }

        [HttpGet("{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            UserDTO? userDTO = await _userService.GetUserAsync(userId);
            if (userDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: DomainErrors.User.NotFound.Message
                );
            }

            return Ok(userDTO);
        }

        [HttpGet("{userId}/profile")]
        [Authorize]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserProfile(Guid userId)
        {
            ProfileDTO? profileDTO = await _userService.GetProfileAsync(CallerId, userId);
            if (profileDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: DomainErrors.User.NotFound.Message
                );
            }

            return Ok(profileDTO);
        }

        [HttpGet("search/{searchString}")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<UserDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            string searchString, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = PaginationConstants.DefaultPageSize, 
            [FromQuery] string sortOrder = PaginationConstants.DefaultSortOrder, 
            [FromQuery] string sortBy = PaginationConstants.DefaultUserSearchSortBy
            )
        {
            return Ok(await _userService.SearchUsers(searchString, pageNumber, pageSize, sortOrder, sortBy, Request));
        }

        [HttpPost("username")]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        [ProducesResponseType(typeof(UserDTO), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ProblemDetails))]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameRequestDTO changeUserNameRequestDTO)
        {
            Result<UserDTO> changeUserNameResult = await _userService.ChangeUserName(CallerId, changeUserNameRequestDTO.NewUserName);
            return changeUserNameResult.Match<IActionResult>
            (
                onSuccess: Ok,
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount()
        {
            Result deleteAccountResult = await _userService.DeleteAccount(CallerId);
            return deleteAccountResult.Match<IActionResult>
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
