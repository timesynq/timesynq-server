using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;
using TimesynqServer.Database.Repository.UserRepository;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;
using TimesynqServer.Services.Service.UserService;

namespace TimesynqServer.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me()
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

            UserDTO? userDTO = await _userService.GetUserAsync(callerGuid);
            if (userDTO == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "User not found."
                );
            }

            return Ok(userDTO);
        }


    }
}
