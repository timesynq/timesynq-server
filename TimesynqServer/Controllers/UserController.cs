using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Services.Repository.UserRepository;

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
        public async Task<IActionResult> Me()
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return ErrorResponse(StatusCodes.Status401Unauthorized, "Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            TimesynqUser? user = await _userRepository.GetById(callerGuid);
            if (user == null)
            {
                return ErrorResponse(StatusCodes.Status404NotFound, "User not found");
            }

            return OkResponse(StatusCodes.Status200OK, user.ToUserDTO());
        }


    }
}
