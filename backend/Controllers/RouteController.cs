using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/routes")]
    [Authorize]
    public class RouteController : ControllerBase
    {
        private readonly IRouteService _routes;
        public RouteController(IRouteService routes) => _routes = routes;

        // GET api/routes
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var routes = await _routes.GetAllAsync();
            return Ok(ApiResponse<List<RouteDto>>.Ok(routes));
        }

        // GET api/routes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var route = await _routes.GetByIdAsync(id);
            return route == null
                ? NotFound(ApiResponse<string>.Fail("Route not found."))
                : Ok(ApiResponse<RouteDto>.Ok(route));
        }

        // GET api/routes/{id}/stops
        [HttpGet("{id}/stops")]
        public async Task<IActionResult> GetStops(int id)
        {
            var route = await _routes.GetByIdAsync(id);
            if (route == null) return NotFound(ApiResponse<string>.Fail("Route not found."));
            return Ok(ApiResponse<List<StopDto>>.Ok(route.Stops));
        }

        // POST api/routes  (Admin only)
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateRouteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid data."));
            var route = await _routes.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = route.Id },
                ApiResponse<RouteDto>.Ok(route, "Route created."));
        }

        // POST api/routes/{id}/stops  (Admin only)
        [HttpPost("{id}/stops")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddStop(int id, [FromBody] CreateStopRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<string>.Fail("Invalid stop data."));
            var stop = await _routes.AddStopAsync(id, request);
            return Ok(ApiResponse<StopDto>.Ok(stop, "Stop added."));
        }

        // DELETE api/routes/{id}  (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _routes.DeleteAsync(id);
            return ok ? Ok(ApiResponse<string>.Ok("deleted"))
                      : NotFound(ApiResponse<string>.Fail("Route not found."));
        }

        // DELETE api/routes/stops/{stopId}  (Admin only)
        [HttpDelete("stops/{stopId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            var ok = await _routes.DeleteStopAsync(stopId);
            return ok ? Ok(ApiResponse<string>.Ok("Stop deleted."))
                      : NotFound(ApiResponse<string>.Fail("Stop not found."));
        }
    }
}
