using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface ILocationService
    {
        Task<LocationUpdateResponse> SaveLocationAsync(LocationUpdateRequest request);
    }

    public class LocationService : ILocationService
    {
        private readonly AppDbContext _db;

        public LocationService(AppDbContext db) => _db = db;

        public async Task<LocationUpdateResponse> SaveLocationAsync(LocationUpdateRequest request)
        {
            // 1. Validate trip is actually InProgress
            var trip = await _db.Trips
                .FirstOrDefaultAsync(t => t.Id == request.TripId && t.Status == "InProgress");

            if (trip == null)
                return new LocationUpdateResponse
                {
                    Success = false,
                    Message = "No active trip found with that ID."
                };

            // 2. Save to LocationLogs (the data warehouse fact table)
            var log = new LocationLog
            {
                BusId          = request.BusId,
                TripId         = request.TripId,
                Latitude       = request.Latitude,
                Longitude      = request.Longitude,
                SpeedKmph      = request.SpeedKmph,
                HeadingDegrees = request.HeadingDegrees,
                AccuracyMeters = request.AccuracyMeters,
                Timestamp      = DateTime.UtcNow
            };

            _db.LocationLogs.Add(log);

            // 3. Update Bus cached location (fast lookup for student app polling)
            var bus = await _db.Buses.FindAsync(request.BusId);
            if (bus != null)
            {
                bus.LastLatitude  = request.Latitude;
                bus.LastLongitude = request.Longitude;
                bus.LastSpeedKmph = request.SpeedKmph;
                bus.LastUpdated   = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return new LocationUpdateResponse
            {
                Success   = true,
                Message   = "Location saved",
                LogId     = log.Id,
                Timestamp = log.Timestamp
            };
        }
    }

    // ── ETA Calculation Service ──────────────────────────────
    public interface IEtaService
    {
        Task<double?> CalculateEtaAsync(int busId, int routeId);
    }

    public class EtaService : IEtaService
    {
        private readonly AppDbContext _db;

        public EtaService(AppDbContext db) => _db = db;

        public async Task<double?> CalculateEtaAsync(int busId, int routeId)
        {
            var bus = await _db.Buses.FindAsync(busId);
            if (bus?.LastLatitude == null) return null;

            // Get all stops on route ordered by StopOrder
            var stops = await _db.Stops
                .Where(s => s.RouteId == routeId)
                .OrderBy(s => s.StopOrder)
                .ToListAsync();

            if (!stops.Any()) return null;

            // Find the next stop the bus hasn't reached yet
            var lastStop = stops.Last();
            double distanceKm = HaversineKm(
                (double)bus.LastLatitude,
                (double)bus.LastLongitude!,
                (double)lastStop.Latitude,
                (double)lastStop.Longitude
            );

            double speedKmph = bus.LastSpeedKmph > 0 ? bus.LastSpeedKmph.Value : 25.0; // default 25 if stopped
            double etaHours  = distanceKm / speedKmph;
            double etaMins   = etaHours * 60;

            return Math.Round(etaMins, 1);
        }

        // Haversine formula — calculates distance between two GPS coords
        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth radius in km
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}
