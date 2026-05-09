using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.Models;

namespace SmartBusAPI.Helpers
{
    /// <summary>
    /// Call DataSeeder.SeedAsync(app) at the end of Program.cs
    /// to pre-populate the DB on first run. Safe to call repeatedly
    /// — skips if data already exists.
    /// </summary>
    public static class DataSeeder
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await db.Database.MigrateAsync();

            // ── Skip if already seeded ───────────────────────
            if (await db.Users.AnyAsync()) return;

            // ── Users ────────────────────────────────────────
            var admin = new User
            {
                Name         = "Admin Rajesh",
                Email        = "admin@college.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role         = "Admin",
                PhoneNumber  = "9876543210",
                IsActive     = true
            };
            var driver1 = new User
            {
                Name         = "Driver Mohan",
                Email        = "mohan@college.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Driver@123"),
                Role         = "Driver",
                PhoneNumber  = "9876500001",
                IsActive     = true
            };
            var driver2 = new User
            {
                Name         = "Driver Suresh",
                Email        = "suresh@college.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Driver@123"),
                Role         = "Driver",
                PhoneNumber  = "9876500002",
                IsActive     = true
            };
            var driver3 = new User
            {
                Name         = "Driver Ramesh",
                Email        = "ramesh@college.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Driver@123"),
                Role         = "Driver",
                PhoneNumber  = "9876500003",
                IsActive     = true
            };
            var student1 = new User
            {
                Name         = "Student Priya",
                Email        = "priya@student.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role         = "Student",
                PhoneNumber  = "9000000001",
                IsActive     = true
            };
            var student2 = new User
            {
                Name         = "Student Arjun",
                Email        = "arjun@student.edu",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
                Role         = "Student",
                PhoneNumber  = "9000000002",
                IsActive     = true
            };

            db.Users.AddRange(admin, driver1, driver2, driver3, student1, student2);
            await db.SaveChangesAsync();

            // ── Routes ───────────────────────────────────────
            var routeA = new BusRoute
            {
                RouteName      = "Route A – Vijay Nagar",
                Description    = "Vijay Nagar → LIG Square → College",
                ScheduledStart = new TimeSpan(7, 30, 0),
                ScheduledEnd   = new TimeSpan(8, 30, 0),
                IsActive       = true
            };
            var routeB = new BusRoute
            {
                RouteName      = "Route B – Palasia",
                Description    = "Palasia → Rajwada → College",
                ScheduledStart = new TimeSpan(7, 45, 0),
                ScheduledEnd   = new TimeSpan(8, 45, 0),
                IsActive       = true
            };
            var routeC = new BusRoute
            {
                RouteName      = "Route C – Scheme 54",
                Description    = "Scheme 54 → Sapna Sangeeta → College",
                ScheduledStart = new TimeSpan(7, 15, 0),
                ScheduledEnd   = new TimeSpan(8, 15, 0),
                IsActive       = true
            };

            db.Routes.AddRange(routeA, routeB, routeC);
            await db.SaveChangesAsync();

            // ── Stops for Route A ────────────────────────────
            db.Stops.AddRange(
                new Stop { RouteId = routeA.Id, StopName = "Vijay Nagar Square",  Latitude = 22.7533m, Longitude = 75.8937m, StopOrder = 1, LandmarkNote = "Near D-Mart" },
                new Stop { RouteId = routeA.Id, StopName = "Annapurna Road",      Latitude = 22.7489m, Longitude = 75.8869m, StopOrder = 2, LandmarkNote = "Near Annapurna Temple" },
                new Stop { RouteId = routeA.Id, StopName = "LIG Square",          Latitude = 22.7404m, Longitude = 75.8779m, StopOrder = 3, LandmarkNote = "Main LIG bus stop" },
                new Stop { RouteId = routeA.Id, StopName = "Rajendra Nagar",      Latitude = 22.7355m, Longitude = 75.8701m, StopOrder = 4, LandmarkNote = "Near garden" },
                new Stop { RouteId = routeA.Id, StopName = "College Main Gate",   Latitude = 22.7196m, Longitude = 75.8577m, StopOrder = 5, LandmarkNote = "Destination" }
            );

            // ── Stops for Route B ────────────────────────────
            db.Stops.AddRange(
                new Stop { RouteId = routeB.Id, StopName = "Palasia Chouraha",  Latitude = 22.7268m, Longitude = 75.8878m, StopOrder = 1, LandmarkNote = "Near Curewell Hospital" },
                new Stop { RouteId = routeB.Id, StopName = "MG Road",          Latitude = 22.7230m, Longitude = 75.8821m, StopOrder = 2, LandmarkNote = "MG Road crossing" },
                new Stop { RouteId = routeB.Id, StopName = "Rajwada",          Latitude = 22.7197m, Longitude = 75.8571m, StopOrder = 3, LandmarkNote = "Historical Rajwada" },
                new Stop { RouteId = routeB.Id, StopName = "Bada Sarafa",      Latitude = 22.7173m, Longitude = 75.8567m, StopOrder = 4, LandmarkNote = "Market area" },
                new Stop { RouteId = routeB.Id, StopName = "College Main Gate",Latitude = 22.7196m, Longitude = 75.8577m, StopOrder = 5, LandmarkNote = "Destination" }
            );

            // ── Stops for Route C ────────────────────────────
            db.Stops.AddRange(
                new Stop { RouteId = routeC.Id, StopName = "Scheme 54 Chouraha", Latitude = 22.7618m, Longitude = 75.8740m, StopOrder = 1, LandmarkNote = "Near petrol pump" },
                new Stop { RouteId = routeC.Id, StopName = "Sapna Sangeeta",     Latitude = 22.7524m, Longitude = 75.8766m, StopOrder = 2 },
                new Stop { RouteId = routeC.Id, StopName = "Navlakha Square",    Latitude = 22.7432m, Longitude = 75.8701m, StopOrder = 3, LandmarkNote = "Near SBI bank" },
                new Stop { RouteId = routeC.Id, StopName = "Kalani Nagar",       Latitude = 22.7340m, Longitude = 75.8636m, StopOrder = 4 },
                new Stop { RouteId = routeC.Id, StopName = "College Main Gate",  Latitude = 22.7196m, Longitude = 75.8577m, StopOrder = 5, LandmarkNote = "Destination" }
            );

            await db.SaveChangesAsync();

            // ── Buses ────────────────────────────────────────
            db.Buses.AddRange(
                new Bus { BusNumber = "BUS-01", LicensePlate = "MP09-AB-1234", CapacitySeats = 45, AssignedDriverId = driver1.Id, AssignedRouteId = routeA.Id },
                new Bus { BusNumber = "BUS-02", LicensePlate = "MP09-CD-5678", CapacitySeats = 40, AssignedDriverId = driver2.Id, AssignedRouteId = routeB.Id },
                new Bus { BusNumber = "BUS-03", LicensePlate = "MP09-EF-9012", CapacitySeats = 50, AssignedDriverId = driver3.Id, AssignedRouteId = routeC.Id }
            );

            await db.SaveChangesAsync();

            Console.WriteLine("✅ Database seeded successfully.");
        }
    }
}
