using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBusAPI.DTOs;
using SmartBusAPI.Services;

namespace SmartBusAPI.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Policy = "AdminOnly")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reports;
        public ReportController(IReportService reports) => _reports = reports;

        // GET api/reports/dashboard
        // Admin panel KPI summary cards
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var summary = await _reports.GetDashboardSummaryAsync();
            return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary));
        }

        // GET api/reports/delays?days=7
        // Mining Query 1 — delay analysis
        [HttpGet("delays")]
        public async Task<IActionResult> Delays([FromQuery] int days = 7)
        {
            var report = await _reports.GetDelayReportAsync(days);
            return Ok(ApiResponse<List<DelayReportDto>>.Ok(report));
        }

        // GET api/reports/peak-hours?days=30
        // Mining Query 2 — peak usage hours
        [HttpGet("peak-hours")]
        public async Task<IActionResult> PeakHours([FromQuery] int days = 30)
        {
            var report = await _reports.GetPeakHoursAsync(days);
            return Ok(ApiResponse<List<PeakHourDto>>.Ok(report));
        }

        // GET api/reports/route-efficiency
        // Mining Query 3 — route efficiency scores
        [HttpGet("route-efficiency")]
        public async Task<IActionResult> RouteEfficiency()
        {
            var report = await _reports.GetRouteEfficiencyAsync();
            return Ok(ApiResponse<List<RouteEfficiencyDto>>.Ok(report));
        }

        // GET api/reports/weekly-trend
        // Mining Query 4 — 7-day delay trend
        [HttpGet("weekly-trend")]
        public async Task<IActionResult> WeeklyTrend()
        {
            var report = await _reports.GetWeeklyTrendAsync();
            return Ok(ApiResponse<List<WeeklyTrendDto>>.Ok(report));
        }
    }
}
