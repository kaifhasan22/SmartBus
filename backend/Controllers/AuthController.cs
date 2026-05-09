using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly JwtService   _jwt;

        public AuthController(IAuthService auth, JwtService jwt)
        {
            _auth = auth;
            _jwt  = jwt;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

            var result = await _auth.LoginAsync(request);

            if (!result.Success)
                return Unauthorized(ApiResponse<string>.Fail(result.Message));

            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        // POST api/auth/register  (Admin only)
        [HttpPost("register")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid request data."));

            var result = await _auth.RegisterAsync(request);

            if (!result.Success)
                return BadRequest(ApiResponse<string>.Fail(result.Message));

            return CreatedAtAction(nameof(Login), ApiResponse<AuthResponse>.Ok(result, "User registered."));
        }

        // POST api/auth/fcm-token  (update Firebase push token)
        [HttpPost("fcm-token")]
        [Authorize]
        public async Task<IActionResult> UpdateFCMToken([FromBody] UpdateFCMTokenRequest request)
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var ok     = await _auth.UpdateFCMTokenAsync(userId, request.FCMToken);
            return ok ? Ok(ApiResponse<string>.Ok("updated")) : NotFound();
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var role   = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var name   = User.FindFirst("name")?.Value;
            return Ok(ApiResponse<object>.Ok(new { userId, role, name }));
        }
    }
}
