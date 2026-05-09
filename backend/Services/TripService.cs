using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface ITripService
    {
        Task<List<TripDto>> GetAllAsync();
        Task<List<TripDto>> GetActiveTripsAsync();
        Task<List<TripDto>> GetByDriverAsync(int driverId);
        Task<TripDto?>      GetByIdAsync(int id);
        Task<TripDto>       StartTripAsync(int driverId, StartTripRequest request);
        Task<TripDto?>      EndTripAsync(int tripId, int driverId, EndTripRequest request);
    }

    public class TripService : ITripService
    {
        private readonly AppDbContext         _db;
        private readonly INotificationService _notifications;

        public TripService(AppDbContext db, INotificationService notifications)
        {
            _db            = db;
            _notifications = notifications;
        }

        public async Task<List<TripDto>> GetAllAsync()
        {
            return await _db.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => ToDto(t))
                .ToListAsync();
        }

        public async Task<List<TripDto>> GetActiveTripsAsync()
        {
            return await _db.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .Where(t => t.Status == "InProgress")
                .Select(t => ToDto(t))
                .ToListAsync();
        }

        public async Task<List<TripDto>> GetByDriverAsync(int driverId)
        {
            return await _db.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .Where(t => t.DriverId == driverId)
                .OrderByDescending(t => t.StartTime)
                .Take(20)
                .Select(t => ToDto(t))
                .ToListAsync();
        }

        public async Task<TripDto?> GetByIdAsync(int id)
        {
            var trip = await _db.Trips
                .Include(t => t.Bus)
                .Include(t => t.Route)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.Id == id);

            return trip == null ? null : ToDto(trip);
        }

        public async Task<TripDto> StartTripAsync(int driverId, StartTripRequest request)
        {
            // Cancel any orphaned InProgress trips for this driver
            var orphaned = await _db.Trips
                .Where(t => t.DriverId == driverId && t.Status == "InProgress")
                .ToListAsync();

            foreach (var o in orphaned)
            {
                o.Status  = "Cancelled";
                o.EndTime = DateTime.UtcNow;
            }

            var trip = new Trip
            {
                BusId     = request.BusId,
                RouteId   = request.RouteId,
                DriverId  = driverId,
                Status    = "InProgress",
                StartTime = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync();

            // Notify all students about the trip starting
            await _notifications.BroadcastAsync(
                role:    "Student",
                title:   "🚌 Trip Started",
                message: $"Bus {request.BusId} has started its route. Track it now!",
                type:    "TripStarted"
            );

            return (await GetByIdAsync(trip.Id))!;
        }

        public async Task<TripDto?> EndTripAsync(int tripId, int driverId, EndTripRequest request)
        {
            var trip = await _db.Trips
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.Id == tripId
                                       && t.DriverId == driverId
                                       && t.Status == "InProgress");

            if (trip == null) return null;

            trip.EndTime = DateTime.UtcNow;
            trip.Status  = "Completed";
            trip.Notes   = request.Notes;

            // Calculate delay vs scheduled time
            if (trip.StartTime.HasValue)
            {
                var actualMins   = (trip.EndTime.Value - trip.StartTime.Value).TotalMinutes;
                var scheduledEnd = trip.Route.ScheduledEnd;
                var scheduledStart = trip.Route.ScheduledStart;
                var plannedMins  = (scheduledEnd - scheduledStart).TotalMinutes;
                trip.DelayMinutes = Math.Max(0, (int)(actualMins - plannedMins));
            }

            await _db.SaveChangesAsync();
            return (await GetByIdAsync(tripId))!;
        }

        private static TripDto ToDto(Trip t) => new()
        {
            Id           = t.Id,
            BusId        = t.BusId,
            BusNumber    = t.Bus?.BusNumber ?? "",
            RouteId      = t.RouteId,
            RouteName    = t.Route?.RouteName ?? "",
            DriverId     = t.DriverId,
            DriverName   = t.Driver?.Name ?? "",
            Status       = t.Status,
            StartTime    = t.StartTime,
            EndTime      = t.EndTime,
            DelayMinutes = t.DelayMinutes,
            Notes        = t.Notes
        };
    }
}
