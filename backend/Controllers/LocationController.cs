using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/location")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _location;

        public LocationController(ILocationService location)
        {
            _location = location;
        }

        // POST api/location/update
        // Called by Driver App every 5 seconds with GPS coordinates
        // Demonstrates: Client-Server architecture, REST over HTTP/HTTPS (Networking subject)
        [HttpPost("update")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> Update([FromBody] LocationUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid location data."));

            var result = await _location.SaveLocationAsync(request);

            return result.Success
                ? Ok(ApiResponse<LocationUpdateResponse>.Ok(result))
                : BadRequest(ApiResponse<string>.Fail(result.Message));
        }
    }
}
