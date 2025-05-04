using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfApi.DBcontext;
using ProfApi.Models;
using ProfApi.Models.ScrollDTO;
using ProfApi.Models.UserDTO;
using System.Security.Claims;

namespace ProfApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowsController : ControllerBase
    {
        private readonly ProfDbContext _context;
        private readonly ILogger<FollowsController> _logger;

        public FollowsController(ProfDbContext context, ILogger<FollowsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("followers/{userId}")]
        public async Task<IActionResult> GetFollowers(int userId, [FromQuery] int lastUserId = 0, [FromQuery] string name = null)
        {
            int pageSize = 10;

            var followersUser = _context.Followers
                .Where(f => f.FollowingId == userId)
                .Select(f => f.Followero);

            if (!string.IsNullOrEmpty(name))
            {
                followersUser = followersUser.Where(u => u.UserName.Contains(name));
            }

            var follows = followersUser.Where(u => lastUserId == 0 || u.UserId > lastUserId)
                .Select(u => new UserListDTO
                {
                    UserId = u.UserId,
                    ProfilePicture = u.ProfilePicture,
                    UserName = u.UserName
                });

            var followers = await follows
                .Take(pageSize)
                .ToListAsync();

            var totalFollowers = await _context.Followers
                .Where(f => f.FollowingId == userId)
                .CountAsync();

            bool hasMore = followers.Count < totalFollowers;

            int lastId = followers.LastOrDefault()?.UserId ?? 0;

            ScrollDTO<UserListDTO> result = new()
            {
                Data = followers,
                TotalRecords = totalFollowers,
                LastId = lastId,
                HasMore = hasMore
            };

            _logger.LogInformation("Mostrar seguidores");

            return Ok(result);
        }

        [HttpGet("following/{userId}")]
        public async Task<IActionResult> GetFollowing(int userId, [FromQuery] int lastUserId = 0, [FromQuery] string name = null)
        {
            int pageSize = 10;

            var followingUser = _context.Followers
                .Where(f => f.FollowerId == userId) 
                .Select(f => f.Following);

     
            if (!string.IsNullOrEmpty(name))
            {
                followingUser = followingUser.Where(u => u.UserName.Contains(name)); 
            }

            var following = followingUser
                .Where(u => lastUserId == 0 || u.UserId > lastUserId) 
                .Select(u => new UserListDTO
                {
                    UserId = u.UserId,
                    ProfilePicture = u.ProfilePicture,
                    UserName = u.UserName
                });

            var followingList = await following
                .Take(pageSize) 
                .ToListAsync();

            var totalFollowing = await _context.Followers
                .Where(f => f.FollowerId == userId)
                .CountAsync();

            bool hasMore = followingList.Count < totalFollowing;

            int lastId = followingList.LastOrDefault()?.UserId ?? 0;

            ScrollDTO<UserListDTO> result = new()
            {
                Data = followingList,
                TotalRecords = totalFollowing,
                LastId = lastId,
                HasMore = hasMore
            };

            _logger.LogInformation("Mostrar siguiendo");

            return Ok(result);
        }

        [Authorize]
        [HttpPost("StartFollowing")]
        public async Task<IActionResult> StartFollowing(int profileId) {

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (userId != profileId)
            {
                var userFollowing = await _context.Users.FindAsync(profileId);
                if (userFollowing != null)
                {
                    var follows = await _context.Followers.FirstOrDefaultAsync(r => r.FollowerId == userId && r.FollowingId == profileId);
                    if (follows == null)
                    {
                        var user = await _context.Users.FindAsync(userId);
                        Follower follower = new()
                        {
                            FollowerId = userId,
                            FollowingId = profileId
                        };
                        _logger.LogWarning("Following");
                        user.CountFollowing += 1;
                        userFollowing.CountFollowers += 1;

                        _context.Followers.Add(follower);
                        _context.Users.Update(user);
                        _context.Users.Update(userFollowing);
                        await _context.SaveChangesAsync();
                        return Ok();
                    }
                    else
                    {
                        _logger.LogWarning("Ya sigue a este usuario");
                        return NotFound("Ya sigue al usuario");
                    }
                }
                else
                {
                    _logger.LogWarning("Usuario no encontrado");
                    return NotFound("Usuario no encontrado");
                }
            }
            else {
                _logger.LogWarning("No puedes seguirte a ti mismo");
                return BadRequest("No puedes seguirte a tí mismo");
            }

        }

        [Authorize]
        [HttpDelete("StopFollowing")]
        public async Task<IActionResult> StopFollowing(int profileId) {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFollowing = await _context.Users.FindAsync(profileId);
            if (userFollowing != null)
            {
                var follows = await _context.Followers.FirstOrDefaultAsync(r => r.FollowerId == userId && r.FollowingId == profileId);
                if (follows != null)
                {
                    var user = await _context.Users.FindAsync(userId);

                    user.CountFollowing -= 1;
                    userFollowing.CountFollowers -= 1;

                    _logger.LogWarning("Paró de seguir");
                    _context.Followers.Remove(follows);
                    _context.Users.Update(user);
                    _context.Users.Update(userFollowing);
                    await _context.SaveChangesAsync();
                    return Ok();

                }
                else
                {
                    _logger.LogWarning("No sigue a este usuario");
                    return NotFound("No sigue al usuario");
                }
            }
            else
            {
                _logger.LogWarning("Usuario no encontrado");
                return NotFound("Usuario no encontrado");
            }

        }

        [Authorize]
        [HttpDelete("DropFollower")]
        public async Task<IActionResult> DropFollower(int profileId) {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFollowing = await _context.Users.FindAsync(profileId);
            if (userFollowing != null)
            {
                var follows = await _context.Followers.FirstOrDefaultAsync(r => r.FollowerId == profileId && r.FollowingId == userId);
                if (follows != null)
                {
                    var user = await _context.Users.FindAsync(userId);
                    _logger.LogWarning("Paró de seguirte");
                    user.CountFollowers -= 1;
                    userFollowing.CountFollowing -= 1;

                    _logger.LogWarning("Paró de seguir");
                    _context.Followers.Remove(follows);
                    _context.Users.Update(user);
                    _context.Users.Update(userFollowing);
                    await _context.SaveChangesAsync();
                    return Ok();

                }
                else
                {
                    _logger.LogWarning("Usuario no te sigue");
                    return NotFound("No sigues al usuario");
                }
            }
            else
            {
                _logger.LogWarning("Usuario no encontrado");
                return NotFound("Usuario no encontrado");
            }
        }
    }
}
