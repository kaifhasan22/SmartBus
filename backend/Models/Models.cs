using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBusAPI.Models
{
    // ══════════════════════════════════════════════════════════
    //  USER
    // ══════════════════════════════════════════════════════════
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Student"; // Admin | Driver | Student

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? FCMToken { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Trip>         Trips         { get; set; } = new List<Trip>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public Bus?                       AssignedBus   { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  ROUTE
    // ══════════════════════════════════════════════════════════
    public class BusRoute
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string RouteName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public TimeSpan ScheduledStart { get; set; }
        public TimeSpan ScheduledEnd   { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Stop>  Stops { get; set; } = new List<Stop>();
        public ICollection<Bus>   Buses { get; set; } = new List<Bus>();
        public ICollection<Trip>  Trips { get; set; } = new List<Trip>();
    }

    // ══════════════════════════════════════════════════════════
    //  STOP
    // ══════════════════════════════════════════════════════════
    public class Stop
    {
        [Key]
        public int Id { get; set; }

        public int RouteId { get; set; }

        [Required, MaxLength(100)]
        public string StopName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }

        public int StopOrder { get; set; }

        [MaxLength(200)]
        public string? LandmarkNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("RouteId")]
        public BusRoute Route { get; set; } = null!;
    }

    // ══════════════════════════════════════════════════════════
    //  BUS
    // ══════════════════════════════════════════════════════════
    public class Bus
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string BusNumber { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        public int CapacitySeats { get; set; } = 40;

        public int? AssignedDriverId { get; set; }
        public int? AssignedRouteId  { get; set; }

        public bool IsActive { get; set; } = true;

        // Cached last known GPS (updated on every ping)
        [Column(TypeName = "decimal(10,7)")]
        public decimal? LastLatitude  { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal? LastLongitude { get; set; }

        public double? LastSpeedKmph { get; set; }
        public DateTime? LastUpdated { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("AssignedDriverId")]
        public User? Driver { get; set; }

        [ForeignKey("AssignedRouteId")]
        public BusRoute? Route { get; set; }

        public ICollection<Trip>        Trips        { get; set; } = new List<Trip>();
        public ICollection<LocationLog> LocationLogs { get; set; } = new List<LocationLog>();
    }

    // ══════════════════════════════════════════════════════════
    //  TRIP
    // ══════════════════════════════════════════════════════════
    public class Trip
    {
        [Key]
        public int Id { get; set; }

        public int BusId    { get; set; }
        public int RouteId  { get; set; }
        public int DriverId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Scheduled"; // Scheduled|InProgress|Completed|Cancelled

        public DateTime? StartTime    { get; set; }
        public DateTime? EndTime      { get; set; }
        public int       DelayMinutes { get; set; } = 0;

        [MaxLength(300)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("BusId")]    public Bus      Bus    { get; set; } = null!;
        [ForeignKey("RouteId")]  public BusRoute Route  { get; set; } = null!;
        [ForeignKey("DriverId")] public User     Driver { get; set; } = null!;

        public ICollection<LocationLog> LocationLogs { get; set; } = new List<LocationLog>();
    }

    // ══════════════════════════════════════════════════════════
    //  LOCATION LOG  (Data Warehouse Fact Table)
    // ══════════════════════════════════════════════════════════
    public class LocationLog
    {
        [Key]
        public long Id { get; set; }

        public int BusId  { get; set; }
        public int TripId { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude  { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }

        public double SpeedKmph      { get; set; } = 0;
        public double? HeadingDegrees { get; set; }
        public double? AccuracyMeters { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("BusId")]  public Bus  Bus  { get; set; } = null!;
        [ForeignKey("TripId")] public Trip Trip { get; set; } = null!;
    }

    // ══════════════════════════════════════════════════════════
    //  NOTIFICATION
    // ══════════════════════════════════════════════════════════
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Type { get; set; } = "General"; // BusDelay|BusArrived|TripStarted|General

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
