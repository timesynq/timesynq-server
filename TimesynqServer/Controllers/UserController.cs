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
    public class UserController : TimesynqController
    {

        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(ResponseDTO<UserDTO>), StatusCodes.Status200OK)]
        [ProducesErrorResponseType(typeof(ResponseDTO<object>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Me()
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return ErrorResponse(StatusCodes.Status401Unauthorized, "Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            UserProjection? userProjection = await _userRepository.GetByIdAsync(callerGuid);
            if (userProjection == null)
            {
                return ErrorResponse(StatusCodes.Status404NotFound, "User not found");
            }

            return OkResponse(StatusCodes.Status200OK, UserDTO.FromProjection(userProjection));
        }


    }
}
