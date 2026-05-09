using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface IRouteService
    {
        Task<List<RouteDto>> GetAllAsync();
        Task<RouteDto?>      GetByIdAsync(int id);
        Task<RouteDto>       CreateAsync(CreateRouteRequest request);
        Task<bool>           DeleteAsync(int id);
        Task<StopDto>        AddStopAsync(int routeId, CreateStopRequest request);
        Task<bool>           DeleteStopAsync(int stopId);
    }

    public class RouteService : IRouteService
    {
        private readonly AppDbContext _db;
        public RouteService(AppDbContext db) => _db = db;

        public async Task<List<RouteDto>> GetAllAsync()
        {
            return await _db.Routes
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .Where(r => r.IsActive)
                .Select(r => ToDto(r))
                .ToListAsync();
        }

        public async Task<RouteDto?> GetByIdAsync(int id)
        {
            var r = await _db.Routes
                .Include(r => r.Stops.OrderBy(s => s.StopOrder))
                .FirstOrDefaultAsync(r => r.Id == id);
            return r == null ? null : ToDto(r);
        }

        public async Task<RouteDto> CreateAsync(CreateRouteRequest request)
        {
            var route = new BusRoute
            {
                RouteName      = request.RouteName,
                Description    = request.Description,
                ScheduledStart = TimeSpan.Parse(request.ScheduledStart),
                ScheduledEnd   = TimeSpan.Parse(request.ScheduledEnd),
                IsActive       = true,
                CreatedAt      = DateTime.UtcNow
            };

            _db.Routes.Add(route);
            await _db.SaveChangesAsync();
            return (await GetByIdAsync(route.Id))!;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var route = await _db.Routes.FindAsync(id);
            if (route == null) return false;
            route.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<StopDto> AddStopAsync(int routeId, CreateStopRequest request)
        {
            var stop = new Stop
            {
                RouteId      = routeId,
                StopName     = request.StopName,
                Latitude     = request.Latitude,
                Longitude    = request.Longitude,
                StopOrder    = request.StopOrder,
                LandmarkNote = request.LandmarkNote,
                CreatedAt    = DateTime.UtcNow
            };

            _db.Stops.Add(stop);
            await _db.SaveChangesAsync();

            return new StopDto
            {
                Id           = stop.Id,
                StopName     = stop.StopName,
                Latitude     = stop.Latitude,
                Longitude    = stop.Longitude,
                StopOrder    = stop.StopOrder,
                LandmarkNote = stop.LandmarkNote
            };
        }

        public async Task<bool> DeleteStopAsync(int stopId)
        {
            var stop = await _db.Stops.FindAsync(stopId);
            if (stop == null) return false;
            _db.Stops.Remove(stop);
            await _db.SaveChangesAsync();
            return true;
        }

        private static RouteDto ToDto(BusRoute r) => new()
        {
            Id             = r.Id,
            RouteName      = r.RouteName,
            Description    = r.Description,
            ScheduledStart = r.ScheduledStart.ToString(@"hh\:mm"),
            ScheduledEnd   = r.ScheduledEnd.ToString(@"hh\:mm"),
            IsActive       = r.IsActive,
            StopCount      = r.Stops.Count,
            Stops          = r.Stops.Select(s => new StopDto
            {
                Id           = s.Id,
                StopName     = s.StopName,
                Latitude     = s.Latitude,
                Longitude    = s.Longitude,
                StopOrder    = s.StopOrder,
                LandmarkNote = s.LandmarkNote
            }).ToList()
        };
    }
}
