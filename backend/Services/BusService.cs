using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface IBusService
    {
        Task<List<BusDto>>     GetAllAsync();
        Task<BusDto?>          GetByIdAsync(int id);
        Task<BusLocationDto?>  GetLocationAsync(int busId);
        Task<List<BusLocationDto>> GetAllLocationsAsync();
        Task<BusDto>           CreateAsync(CreateBusRequest request);
        Task<BusDto?>          UpdateAsync(int id, UpdateBusRequest request);
        Task<bool>             DeleteAsync(int id);
    }

    public class BusService : IBusService
    {
        private readonly AppDbContext _db;

        public BusService(AppDbContext db) => _db = db;

        public async Task<List<BusDto>> GetAllAsync()
        {
            return await _db.Buses
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .Include(b => b.Trips.Where(t => t.Status == "InProgress"))
                .Select(b => ToDto(b))
                .ToListAsync();
        }

        public async Task<BusDto?> GetByIdAsync(int id)
        {
            var b = await _db.Buses
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(b => b.Id == id);

            return b == null ? null : ToDto(b);
        }

        public async Task<BusLocationDto?> GetLocationAsync(int busId)
        {
            var bus = await _db.Buses
                .Include(b => b.Trips.Where(t => t.Status == "InProgress"))
                .FirstOrDefaultAsync(b => b.Id == busId && b.IsActive);

            if (bus == null || bus.LastLatitude == null) return null;

            var activeTrip = bus.Trips.FirstOrDefault(t => t.Status == "InProgress");

            return new BusLocationDto
            {
                BusId       = bus.Id,
                BusNumber   = bus.BusNumber,
                Latitude    = bus.LastLatitude.Value,
                Longitude   = bus.LastLongitude!.Value,
                SpeedKmph   = bus.LastSpeedKmph ?? 0,
                LastUpdated = bus.LastUpdated ?? DateTime.UtcNow,
                TripStatus  = activeTrip?.Status ?? "No Active Trip"
            };
        }

        public async Task<List<BusLocationDto>> GetAllLocationsAsync()
        {
            return await _db.Buses
                .Where(b => b.IsActive && b.LastLatitude != null)
                .Include(b => b.Trips.Where(t => t.Status == "InProgress"))
                .Select(b => new BusLocationDto
                {
                    BusId       = b.Id,
                    BusNumber   = b.BusNumber,
                    Latitude    = b.LastLatitude!.Value,
                    Longitude   = b.LastLongitude!.Value,
                    SpeedKmph   = b.LastSpeedKmph ?? 0,
                    LastUpdated = b.LastUpdated ?? DateTime.UtcNow,
                    TripStatus  = b.Trips.Any() ? "InProgress" : "Parked"
                })
                .ToListAsync();
        }

        public async Task<BusDto> CreateAsync(CreateBusRequest request)
        {
            var bus = new Bus
            {
                BusNumber        = request.BusNumber,
                LicensePlate     = request.LicensePlate,
                CapacitySeats    = request.CapacitySeats,
                AssignedDriverId = request.AssignedDriverId,
                AssignedRouteId  = request.AssignedRouteId,
                IsActive         = true,
                CreatedAt        = DateTime.UtcNow
            };

            _db.Buses.Add(bus);
            await _db.SaveChangesAsync();

            return (await GetByIdAsync(bus.Id))!;
        }

        public async Task<BusDto?> UpdateAsync(int id, UpdateBusRequest request)
        {
            var bus = await _db.Buses.FindAsync(id);
            if (bus == null) return null;

            if (request.BusNumber        != null) bus.BusNumber        = request.BusNumber;
            if (request.LicensePlate     != null) bus.LicensePlate     = request.LicensePlate;
            if (request.CapacitySeats    != null) bus.CapacitySeats    = request.CapacitySeats.Value;
            if (request.AssignedDriverId != null) bus.AssignedDriverId = request.AssignedDriverId;
            if (request.AssignedRouteId  != null) bus.AssignedRouteId  = request.AssignedRouteId;
            if (request.IsActive         != null) bus.IsActive         = request.IsActive.Value;

            await _db.SaveChangesAsync();
            return (await GetByIdAsync(id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bus = await _db.Buses.FindAsync(id);
            if (bus == null) return false;

            bus.IsActive = false; // soft delete
            await _db.SaveChangesAsync();
            return true;
        }

        private static BusDto ToDto(Bus b) => new()
        {
            Id               = b.Id,
            BusNumber        = b.BusNumber,
            LicensePlate     = b.LicensePlate,
            CapacitySeats    = b.CapacitySeats,
            AssignedDriverId = b.AssignedDriverId,
            DriverName       = b.Driver?.Name,
            AssignedRouteId  = b.AssignedRouteId,
            RouteName        = b.Route?.RouteName,
            IsActive         = b.IsActive,
            LastLatitude     = b.LastLatitude,
            LastLongitude    = b.LastLongitude,
            LastSpeedKmph    = b.LastSpeedKmph,
            LastUpdated      = b.LastUpdated,
            TripStatus       = b.Trips.Any(t => t.Status == "InProgress") ? "InProgress" : "Parked"
        };
    }
}
