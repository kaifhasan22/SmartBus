using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Models;

namespace SmartBusAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ───────────────────────────────────────────
        public DbSet<User>        Users        { get; set; }
        public DbSet<BusRoute>    Routes       { get; set; }
        public DbSet<Stop>        Stops        { get; set; }
        public DbSet<Bus>         Buses        { get; set; }
        public DbSet<Trip>        Trips        { get; set; }
        public DbSet<LocationLog> LocationLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ────────────────────────────────────────
            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.Role)
                 .HasConversion<string>();
            });

            // ── Bus ─────────────────────────────────────────
            modelBuilder.Entity<Bus>(e =>
            {
                e.HasIndex(b => b.BusNumber).IsUnique();
                e.HasIndex(b => b.LicensePlate).IsUnique();

                // Driver FK — no cascade (driver can be unassigned)
                e.HasOne(b => b.Driver)
                 .WithOne(u => u.AssignedBus)
                 .HasForeignKey<Bus>(b => b.AssignedDriverId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Route FK
                e.HasOne(b => b.Route)
                 .WithMany(r => r.Buses)
                 .HasForeignKey(b => b.AssignedRouteId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ── Stop ────────────────────────────────────────
            modelBuilder.Entity<Stop>(e =>
            {
                e.HasOne(s => s.Route)
                 .WithMany(r => r.Stops)
                 .HasForeignKey(s => s.RouteId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Trip ────────────────────────────────────────
            modelBuilder.Entity<Trip>(e =>
            {
                e.HasOne(t => t.Bus)
                 .WithMany(b => b.Trips)
                 .HasForeignKey(t => t.BusId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Route)
                 .WithMany(r => r.Trips)
                 .HasForeignKey(t => t.RouteId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(t => t.Driver)
                 .WithMany(u => u.Trips)
                 .HasForeignKey(t => t.DriverId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ── LocationLog ─────────────────────────────────
            modelBuilder.Entity<LocationLog>(e =>
            {
                e.HasOne(l => l.Bus)
                 .WithMany(b => b.LocationLogs)
                 .HasForeignKey(l => l.BusId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(l => l.Trip)
                 .WithMany(t => t.LocationLogs)
                 .HasForeignKey(l => l.TripId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Index for fast time-range queries (mining)
                e.HasIndex(l => new { l.BusId, l.Timestamp });
                e.HasIndex(l => l.TripId);
            });

            // ── Notification ────────────────────────────────
            modelBuilder.Entity<Notification>(e =>
            {
                e.HasOne(n => n.User)
                 .WithMany(u => u.Notifications)
                 .HasForeignKey(n => n.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
