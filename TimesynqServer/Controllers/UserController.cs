using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesynqServer.Application.DTO;
using TimesynqServer.Application.Service.UserService;
using TimesynqServer.Common;

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
            UserDTO? userDTO = await _userService.GetUserAsync(CallerGuid);
            if (userDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: DomainErrors.User.NotFound.Message
                );
            }

            return Ok(userDTO);
        }


    }
}
