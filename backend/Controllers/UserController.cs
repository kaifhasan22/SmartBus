using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService   _jwt;

        public UserController(AppDbContext db, JwtService jwt)
        {
            _db  = db;
            _jwt = jwt;
        }

        // GET api/users  (Admin — all users)
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll([FromQuery] string? role = null)
        {
            var query = _db.Users.Where(u => u.IsActive);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            var users = await query
                .OrderBy(u => u.Name)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    u.PhoneNumber,
                    u.IsActive,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(users));
        }

        // GET api/users/drivers  (Admin — all active drivers)
        [HttpGet("drivers")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetDrivers()
        {
            var drivers = await _db.Users
                .Where(u => u.Role == "Driver" && u.IsActive)
                .Include(u => u.AssignedBus)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.PhoneNumber,
                    AssignedBus = u.AssignedBus == null ? null : u.AssignedBus.BusNumber,
                    TotalTrips  = u.Trips.Count
                })
                .ToListAsync();

            return Ok(ApiResponse<object>.Ok(drivers));
        }

        // GET api/users/me  (any authenticated user — their own profile)
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var user   = await _db.Users.FindAsync(userId);

            if (user == null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            return Ok(ApiResponse<object>.Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.IsActive,
                user.CreatedAt
            }));
        }

        // PUT api/users/{id}/deactivate  (Admin only)
        [HttpPut("{id}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            user.IsActive  = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("User deactivated."));
        }

        // PUT api/users/{id}/activate  (Admin only)
        [HttpPut("{id}/activate")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("User not found."));

            user.IsActive  = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("User activated."));
        }
    }
}
