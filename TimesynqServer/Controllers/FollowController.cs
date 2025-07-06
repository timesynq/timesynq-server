using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TimesynqServer.Database;
using TimesynqServer.Database.Entities;
using TimesynqServer.Extensions;
using TimesynqServer.Models.DTO;
using TimesynqServer.Models.DTO.Request.Follow;
using TimesynqServer.Models.Pagination;

namespace TimesynqServer.Controllers
{
    [Route("follows")]
    [ApiController]
    public class FollowController : ControllerBase
    {

        private TimesynqDbContext _dbContext;

        public FollowController(TimesynqDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{userId}/followers")]
        [Authorize]
        public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowers = await _dbContext.Follows
                .Where(f => f.FolloweeId == userId)
                .CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalFollowers / pageSize);

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserDTO> followers = await _dbContext.Follows
                .Where(f => f.FolloweeId == userId)
                .Include(f => f.Follower)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower!.ToUserDTO())
                .ToListAsync();

            PagedResult<UserDTO> pagedResult = followers.ToPagedResult(pageNumber, pageSize, totalFollowers, totalPages, Request);

            return Ok(pagedResult);
        }

        [HttpGet("{userId}/followees")]
        [Authorize]
        public async Task<IActionResult> GetFollowees(Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            int totalFollowees = await _dbContext.Follows
                .Where(f => f.FollowerId == userId)
                .CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalFollowees / pageSize);

            pageNumber = Math.Clamp(pageNumber, 1, totalPages);

            IEnumerable<UserDTO> followees = await _dbContext.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Followee)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Followee!.ToUserDTO())
                .ToListAsync();

            PagedResult<UserDTO> pagedResult = followees.ToPagedResult(pageNumber, pageSize, totalFollowees, totalPages, Request);

            return Ok(pagedResult);
        }

        [HttpPost]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequestDTO followRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return Unauthorized("Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            if (callerGuid == followRequest.FolloweeGuid)
            {
                return Conflict("Can't follow yourself");
            }

            var followee = await _dbContext.Users.FindAsync(followRequest.FolloweeGuid);
            if (followee == null)
            {
                return NotFound("Followee doesn't exist");
            }

            bool alreadyFollowing = await _dbContext.Follows
                .AnyAsync(f => f.FollowerId == callerGuid && f.FolloweeId == followRequest.FolloweeGuid);

            if (alreadyFollowing)
            {
                return Conflict("Already following this user");
            }

            var follow = new Follow
            {
                FollowerId = callerGuid,
                FolloweeId = followRequest.FolloweeGuid,
            };
            await _dbContext.Follows.AddAsync(follow);
            await _dbContext.SaveChangesAsync();

            return Ok("Followed successfully");
        }

        [HttpDelete]
        [Authorize(Roles = "ConfirmedUser, Admin")]
        public async Task<IActionResult> UnfollowUser([FromBody] UnfollowRequestDTO unfollowRequest)
        {
            string? callerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (callerId == null)
            {
                return Unauthorized("Invalid token");
            }
            Guid callerGuid = Guid.Parse(callerId);

            Follow? follow = await _dbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == callerGuid && f.FolloweeId == unfollowRequest.FolloweeGuid);

            if (follow == null)
            {
                return NotFound("Not following this user");
            }

            _dbContext.Remove(follow);
            await _dbContext.SaveChangesAsync();

            return Ok("Unfollowed successfully");
        }

    }
}
