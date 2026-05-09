using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/buses")]
    [Authorize]
    public class BusController : ControllerBase
    {
        private readonly IBusService _buses;
        private readonly IEtaService _eta;

        public BusController(IBusService buses, IEtaService eta)
        {
            _buses = buses;
            _eta   = eta;
        }

        // GET api/buses
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var buses = await _buses.GetAllAsync();
            return Ok(ApiResponse<List<BusDto>>.Ok(buses));
        }

        // GET api/buses/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bus = await _buses.GetByIdAsync(id);
            return bus == null
                ? NotFound(ApiResponse<string>.Fail("Bus not found."))
                : Ok(ApiResponse<BusDto>.Ok(bus));
        }

        // GET api/buses/{id}/location   ← Student app polls this every 5 seconds
        [HttpGet("{id}/location")]
        public async Task<IActionResult> GetLocation(int id)
        {
            var location = await _buses.GetLocationAsync(id);
            if (location == null)
                return NotFound(ApiResponse<string>.Fail("No location data available."));

            // Attach ETA if bus has an active route
            var bus = await _buses.GetByIdAsync(id);
            if (bus?.AssignedRouteId != null)
            {
                location.EtaMinutes = await _eta.CalculateEtaAsync(id, bus.AssignedRouteId.Value);
            }

            return Ok(ApiResponse<BusLocationDto>.Ok(location));
        }

        // GET api/buses/locations/all   ← Admin live map
        [HttpGet("locations/all")]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _buses.GetAllLocationsAsync();
            return Ok(ApiResponse<List<BusLocationDto>>.Ok(locations));
        }

        // POST api/buses  (Admin only)
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateBusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid data."));

            var bus = await _buses.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = bus.Id },
                ApiResponse<BusDto>.Ok(bus, "Bus created."));
        }

        // PUT api/buses/{id}  (Admin only)
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBusRequest request)
        {
            var bus = await _buses.UpdateAsync(id, request);
            return bus == null
                ? NotFound(ApiResponse<string>.Fail("Bus not found."))
                : Ok(ApiResponse<BusDto>.Ok(bus, "Bus updated."));
        }

        // DELETE api/buses/{id}  (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _buses.DeleteAsync(id);
            return ok
                ? Ok(ApiResponse<string>.Ok("deleted"))
                : NotFound(ApiResponse<string>.Fail("Bus not found."));
        }
    }
}
