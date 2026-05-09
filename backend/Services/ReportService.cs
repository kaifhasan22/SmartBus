using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;

namespace SmartBusAPI.Services
{
    public interface IReportService
    {
        Task<DashboardSummaryDto>     GetDashboardSummaryAsync();
        Task<List<DelayReportDto>>    GetDelayReportAsync(int days = 7);
        Task<List<PeakHourDto>>       GetPeakHoursAsync(int days = 30);
        Task<List<RouteEfficiencyDto>> GetRouteEfficiencyAsync();
        Task<List<WeeklyTrendDto>>    GetWeeklyTrendAsync();
    }

    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) => _db = db;

        // ── Dashboard KPI summary ────────────────────────────
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var today     = DateTime.UtcNow.Date;
            var tomorrow  = today.AddDays(1);

            var activeBuses   = await _db.Buses.CountAsync(b => b.IsActive);
            var tripsToday    = await _db.Trips.CountAsync(t => t.StartTime >= today && t.StartTime < tomorrow);
            var avgDelay      = await _db.Trips
                                    .Where(t => t.Status == "Completed" && t.StartTime >= today)
                                    .AverageAsync(t => (double?)t.DelayMinutes) ?? 0;
            var activeAlerts  = await _db.Trips.CountAsync(t => t.Status == "InProgress" && t.DelayMinutes > 5);
            var totalRoutes   = await _db.Routes.CountAsync(r => r.IsActive);
            var totalStudents = await _db.Users.CountAsync(u => u.Role == "Student" && u.IsActive);
            var totalDrivers  = await _db.Users.CountAsync(u => u.Role == "Driver" && u.IsActive);
            var gpsPings      = await _db.LocationLogs.LongCountAsync();

            return new DashboardSummaryDto
            {
                ActiveBuses   = activeBuses,
                TripsToday    = tripsToday,
                AvgDelayMins  = Math.Round(avgDelay, 1),
                ActiveAlerts  = activeAlerts,
                TotalRoutes   = totalRoutes,
                TotalStudents = totalStudents,
                TotalDrivers  = totalDrivers,
                TotalGpsPings = gpsPings
            };
        }

        // ── Mining Query 1: Delay Analysis ───────────────────
        public async Task<List<DelayReportDto>> GetDelayReportAsync(int days = 7)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            return await _db.Trips
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .Include(t => t.Bus)
                .Where(t => t.Status == "Completed" && t.StartTime >= since)
                .OrderByDescending(t => t.DelayMinutes)
                .Select(t => new DelayReportDto
                {
                    TripId        = t.Id,
                    RouteName     = t.Route.RouteName,
                    DriverName    = t.Driver.Name,
                    BusNumber     = t.Bus.BusNumber,
                    StartTime     = t.StartTime,
                    DelayMinutes  = t.DelayMinutes,
                    DelayCategory = t.DelayMinutes == 0           ? "On Time"
                                  : t.DelayMinutes <= 10         ? "Slight Delay"
                                  : t.DelayMinutes <= 20         ? "Moderate Delay"
                                  : "Severe Delay"
                })
                .ToListAsync();
        }

        // ── Mining Query 2: Peak Usage Hours ─────────────────
        public async Task<List<PeakHourDto>> GetPeakHoursAsync(int days = 30)
        {
            var since = DateTime.UtcNow.AddDays(-days);

            var raw = await _db.Trips
                .Where(t => t.StartTime >= since && t.StartTime != null)
                .ToListAsync(); // bring into memory for time part extraction

            return raw
                .GroupBy(t => t.StartTime!.Value.Hour)
                .Select(g => new PeakHourDto
                {
                    HourOfDay = g.Key,
                    TripCount = g.Count(),
                    Label     = g.Key == 0  ? "12 AM"
                              : g.Key < 12  ? $"{g.Key} AM"
                              : g.Key == 12 ? "12 PM"
                              : $"{g.Key - 12} PM"
                })
                .OrderByDescending(x => x.TripCount)
                .ToList();
        }

        // ── Mining Query 3: Route Efficiency ─────────────────
        public async Task<List<RouteEfficiencyDto>> GetRouteEfficiencyAsync()
        {
            var routes = await _db.Routes
                .Include(r => r.Trips.Where(t => t.Status == "Completed"))
                .Where(r => r.IsActive)
                .ToListAsync();

            var result = new List<RouteEfficiencyDto>();

            foreach (var route in routes)
            {
                var trips      = route.Trips.ToList();
                var totalTrips = trips.Count;
                if (totalTrips == 0) continue;

                // Get avg speed from location logs for trips on this route
                var tripIds    = trips.Select(t => t.Id).ToList();
                var avgSpeed   = await _db.LocationLogs
                    .Where(l => tripIds.Contains(l.TripId) && l.SpeedKmph > 0)
                    .AverageAsync(l => (double?)l.SpeedKmph) ?? 0;

                var delayedTrips = trips.Count(t => t.DelayMinutes > 0);
                var delayRate    = totalTrips > 0 ? (double)delayedTrips / totalTrips * 100 : 0;
                var efficiency   = Math.Max(0, 100 - delayRate);

                result.Add(new RouteEfficiencyDto
                {
                    RouteName      = route.RouteName,
                    TotalTrips     = totalTrips,
                    AvgSpeedKmph   = Math.Round(avgSpeed, 1),
                    DelayedTrips   = delayedTrips,
                    DelayRatePct   = Math.Round(delayRate, 1),
                    EfficiencyScore = Math.Round(efficiency, 1)
                });
            }

            return result.OrderByDescending(r => r.EfficiencyScore).ToList();
        }

        // ── Mining Query 4: Weekly Trend ─────────────────────
        public async Task<List<WeeklyTrendDto>> GetWeeklyTrendAsync()
        {
            var since = DateTime.UtcNow.AddDays(-7).Date;

            var trips = await _db.Trips
                .Where(t => t.Status == "Completed" && t.StartTime >= since)
                .ToListAsync();

            return trips
                .GroupBy(t => t.StartTime!.Value.Date)
                .Select(g => new WeeklyTrendDto
                {
                    TripDate     = g.Key,
                    TotalTrips   = g.Count(),
                    DelayedCount = g.Count(t => t.DelayMinutes > 0),
                    AvgDelayMins = Math.Round(g.Average(t => (double)t.DelayMinutes), 1)
                })
                .OrderBy(x => x.TripDate)
                .ToList();
        }
    }
}
