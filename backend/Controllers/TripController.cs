using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly ITripService _trips;
        private readonly JwtService   _jwt;

        public TripController(ITripService trips, JwtService jwt)
        {
            _trips = trips;
            _jwt   = jwt;
        }

        // GET api/trips  (Admin)
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            var trips = await _trips.GetAllAsync();
            return Ok(ApiResponse<List<TripDto>>.Ok(trips));
        }

        // GET api/trips/active  (Students see active trips)
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var trips = await _trips.GetActiveTripsAsync();
            return Ok(ApiResponse<List<TripDto>>.Ok(trips));
        }

        // GET api/trips/my  (Driver sees their own trips)
        [HttpGet("my")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> GetMyTrips()
        {
            var driverId = _jwt.GetUserIdFromToken(User);
            var trips    = await _trips.GetByDriverAsync(driverId);
            return Ok(ApiResponse<List<TripDto>>.Ok(trips));
        }

        // GET api/trips/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var trip = await _trips.GetByIdAsync(id);
            return trip == null
                ? NotFound(ApiResponse<string>.Fail("Trip not found."))
                : Ok(ApiResponse<TripDto>.Ok(trip));
        }

        // POST api/trips/start  (Driver starts a trip)
        [HttpPost("start")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> Start([FromBody] StartTripRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid data."));

            var driverId = _jwt.GetUserIdFromToken(User);
            var trip     = await _trips.StartTripAsync(driverId, request);

            return CreatedAtAction(nameof(GetById), new { id = trip.Id },
                ApiResponse<TripDto>.Ok(trip, "Trip started."));
        }

        // POST api/trips/{id}/end  (Driver ends a trip)
        [HttpPost("{id}/end")]
        [Authorize(Policy = "DriverOnly")]
        public async Task<IActionResult> End(int id, [FromBody] EndTripRequest request)
        {
            var driverId = _jwt.GetUserIdFromToken(User);
            var trip     = await _trips.EndTripAsync(id, driverId, request);

            return trip == null
                ? NotFound(ApiResponse<string>.Fail("Active trip not found."))
                : Ok(ApiResponse<TripDto>.Ok(trip, "Trip ended."));
        }
    }
}
