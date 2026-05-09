using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifications;
        private readonly JwtService           _jwt;

        public NotificationController(INotificationService notifications, JwtService jwt)
        {
            _notifications = notifications;
            _jwt           = jwt;
        }

        // GET api/notifications/my  — logged-in user's inbox
        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var list   = await _notifications.GetForUserAsync(userId);
            return Ok(ApiResponse<List<NotificationDto>>.Ok(list));
        }

        // POST api/notifications/send  (Admin only — send to one user)
        [HttpPost("send")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid data."));

            await _notifications.SendAsync(request.UserId, request.Title, request.Message, request.Type);
            return Ok(ApiResponse<string>.Ok("Notification sent."));
        }

        // POST api/notifications/broadcast  (Admin only — send to all students or drivers)
        [HttpPost("broadcast")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Broadcast([FromBody] BroadcastNotificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid data."));

            await _notifications.BroadcastAsync(request.Role, request.Title, request.Message, request.Type);
            return Ok(ApiResponse<string>.Ok($"Broadcast sent to all {request.Role}s."));
        }

        // PUT api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var ok     = await _notifications.MarkReadAsync(id, userId);
            return ok
                ? Ok(ApiResponse<string>.Ok("Marked as read."))
                : NotFound(ApiResponse<string>.Fail("Notification not found."));
        }

        // PUT api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = _jwt.GetUserIdFromToken(User);
            var count  = await _notifications.MarkAllReadAsync(userId);
            return Ok(ApiResponse<string>.Ok($"{count} notifications marked as read."));
        }
    }
}
