using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesynqServer.Database.Entities;
using TimesynqServer.Database.Projections;
using TimesynqServer.Database.Repository.UserRepository;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;

namespace TimesynqServer.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

            UserProjection? userProjection = await _userRepository.GetByIdAsync(callerGuid);
            if (userProjection == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    detail: "User not found."
                );
            }

            return Ok(UserDTO.FromProjection(userProjection));
        }


    }
}
