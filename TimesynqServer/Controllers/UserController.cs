using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using TimesynqServer.Database.Entities;
using TimesynqServer.Models.DTO;
using TimesynqServer.Services;

namespace TimesynqServer.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private UserManager<TimesynqUser> _userManager;

        public UserController(UserManager<TimesynqUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequestDTO signUpRequestDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var newUser = new TimesynqUser
                {
                    UserName = signUpRequestDTO.Username,
                    Email = signUpRequestDTO.Email,
                    ProfilePicture = TimesynqRandomizer.GenerateIdenticon(),
                    CreatedOnUTC = DateTime.UtcNow,
                    LastUpdatedOnUTC = DateTime.UtcNow,
                };

                var createdUser = await _userManager.CreateAsync(newUser, signUpRequestDTO.Password!);

                if (!createdUser.Succeeded)
                {
                    return BadRequest(createdUser.Errors);
                }

                var roleResult = await _userManager.AddToRoleAsync(newUser, "UnconfirmedUser");

                if (!roleResult.Succeeded)
                {
                    return BadRequest(roleResult.Errors);
                }

                return Ok("User Created");

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

    }
}
