using System.ComponentModel.DataAnnotations;

namespace SmartBusAPI.DTOs
{
    // ══════════════════════════════════════════════════════════
    //  AUTH DTOs
    // ══════════════════════════════════════════════════════════
    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email    { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string Name     { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email    { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role     { get; set; } = "Student"; // Student|Driver|Admin

        public string? PhoneNumber { get; set; }
    }

    public class AuthResponse
    {
        public bool   Success  { get; set; }
        public string Token    { get; set; } = string.Empty;
        public string Role     { get; set; } = string.Empty;
        public int    UserId   { get; set; }
        public string Name     { get; set; } = string.Empty;
        public string Email    { get; set; } = string.Empty;
        public string Message  { get; set; } = string.Empty;
    }

    public class UpdateFCMTokenRequest
    {
        [Required]
        public string FCMToken { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════
    //  BUS DTOs
    // ══════════════════════════════════════════════════════════
    public class BusDto
    {
        public int     Id               { get; set; }
        public string  BusNumber        { get; set; } = string.Empty;
        public string  LicensePlate     { get; set; } = string.Empty;
        public int     CapacitySeats    { get; set; }
        public int?    AssignedDriverId { get; set; }
        public string? DriverName       { get; set; }
        public int?    AssignedRouteId  { get; set; }
        public string? RouteName        { get; set; }
        public bool    IsActive         { get; set; }
        public decimal? LastLatitude    { get; set; }
        public decimal? LastLongitude   { get; set; }
        public double?  LastSpeedKmph   { get; set; }
        public DateTime? LastUpdated    { get; set; }
        public string  TripStatus       { get; set; } = "No Active Trip";
    }

    public class CreateBusRequest
    {
        [Required, MaxLength(20)]
        public string BusNumber     { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string LicensePlate  { get; set; } = string.Empty;

        public int  CapacitySeats    { get; set; } = 40;
        public int? AssignedDriverId { get; set; }
        public int? AssignedRouteId  { get; set; }
    }

    public class UpdateBusRequest
    {
        public string? BusNumber        { get; set; }
        public string? LicensePlate     { get; set; }
        public int?    CapacitySeats    { get; set; }
        public int?    AssignedDriverId { get; set; }
        public int?    AssignedRouteId  { get; set; }
        public bool?   IsActive         { get; set; }
    }

    public class BusLocationDto
    {
        public int      BusId        { get; set; }
        public string   BusNumber    { get; set; } = string.Empty;
        public decimal  Latitude     { get; set; }
        public decimal  Longitude    { get; set; }
        public double   SpeedKmph    { get; set; }
        public DateTime LastUpdated  { get; set; }
        public double?  EtaMinutes   { get; set; }
        public string?  NextStopName { get; set; }
        public string   TripStatus   { get; set; } = string.Empty;
    }

    // ══════════════════════════════════════════════════════════
    //  ROUTE DTOs
    // ══════════════════════════════════════════════════════════
    public class RouteDto
    {
        public int         Id             { get; set; }
        public string      RouteName      { get; set; } = string.Empty;
        public string?     Description    { get; set; }
        public string      ScheduledStart { get; set; } = string.Empty;
        public string      ScheduledEnd   { get; set; } = string.Empty;
        public bool        IsActive       { get; set; }
        public int         StopCount      { get; set; }
        public List<StopDto> Stops        { get; set; } = new();
    }

    public class StopDto
    {
        public int     Id           { get; set; }
        public string  StopName     { get; set; } = string.Empty;
        public decimal Latitude     { get; set; }
        public decimal Longitude    { get; set; }
        public int     StopOrder    { get; set; }
        public string? LandmarkNote { get; set; }
    }

    public class CreateRouteRequest
    {
        [Required, MaxLength(100)]
        public string RouteName      { get; set; } = string.Empty;
        public string? Description   { get; set; }

        [Required]
        public string ScheduledStart { get; set; } = string.Empty; // "07:30"

        [Required]
        public string ScheduledEnd   { get; set; } = string.Empty; // "08:30"
    }

    public class CreateStopRequest
    {
        [Required, MaxLength(100)]
        public string  StopName     { get; set; } = string.Empty;

        [Required]
        public decimal Latitude     { get; set; }

        [Required]
        public decimal Longitude    { get; set; }

        [Required]
        public int     StopOrder    { get; set; }

        public string? LandmarkNote { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  LOCATION DTOs
    // ══════════════════════════════════════════════════════════
    public class LocationUpdateRequest
    {
        [Required]
        public int BusId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required, Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Required, Range(-180, 180)]
        public decimal Longitude { get; set; }

        public double SpeedKmph       { get; set; } = 0;
        public double? HeadingDegrees { get; set; }
        public double? AccuracyMeters { get; set; }
    }

    public class LocationUpdateResponse
    {
        public bool   Success     { get; set; }
        public string Message     { get; set; } = string.Empty;
        public long   LogId       { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  TRIP DTOs
    // ══════════════════════════════════════════════════════════
    public class TripDto
    {
        public int      Id           { get; set; }
        public int      BusId        { get; set; }
        public string   BusNumber    { get; set; } = string.Empty;
        public int      RouteId      { get; set; }
        public string   RouteName    { get; set; } = string.Empty;
        public int      DriverId     { get; set; }
        public string   DriverName   { get; set; } = string.Empty;
        public string   Status       { get; set; } = string.Empty;
        public DateTime? StartTime   { get; set; }
        public DateTime? EndTime     { get; set; }
        public int      DelayMinutes { get; set; }
        public string?  Notes        { get; set; }
    }

    public class StartTripRequest
    {
        [Required]
        public int BusId   { get; set; }

        [Required]
        public int RouteId { get; set; }
    }

    public class EndTripRequest
    {
        public string? Notes { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  NOTIFICATION DTOs
    // ══════════════════════════════════════════════════════════
    public class NotificationDto
    {
        public int      Id      { get; set; }
        public string   Title   { get; set; } = string.Empty;
        public string   Message { get; set; } = string.Empty;
        public string   Type    { get; set; } = string.Empty;
        public bool     IsRead  { get; set; }
        public DateTime SentAt  { get; set; }
    }

    public class SendNotificationRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "General";
    }

    public class BroadcastNotificationRequest
    {
        [Required]
        public string Role    { get; set; } = "Student"; // broadcast to all students

        [Required, MaxLength(100)]
        public string Title   { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public string Type    { get; set; } = "General";
    }

    // ══════════════════════════════════════════════════════════
    //  REPORT / MINING DTOs
    // ══════════════════════════════════════════════════════════
    public class DelayReportDto
    {
        public int    TripId        { get; set; }
        public string RouteName     { get; set; } = string.Empty;
        public string DriverName    { get; set; } = string.Empty;
        public string BusNumber     { get; set; } = string.Empty;
        public DateTime? StartTime  { get; set; }
        public int    DelayMinutes  { get; set; }
        public string DelayCategory { get; set; } = string.Empty;
    }

    public class PeakHourDto
    {
        public int HourOfDay  { get; set; }
        public int TripCount  { get; set; }
        public string Label   { get; set; } = string.Empty; // "8 AM"
    }

    public class RouteEfficiencyDto
    {
        public string RouteName      { get; set; } = string.Empty;
        public int    TotalTrips     { get; set; }
        public double AvgSpeedKmph   { get; set; }
        public int    DelayedTrips   { get; set; }
        public double DelayRatePct   { get; set; }
        public double EfficiencyScore { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int    ActiveBuses    { get; set; }
        public int    TripsToday     { get; set; }
        public double AvgDelayMins   { get; set; }
        public int    ActiveAlerts   { get; set; }
        public int    TotalRoutes    { get; set; }
        public int    TotalStudents  { get; set; }
        public int    TotalDrivers   { get; set; }
        public long   TotalGpsPings  { get; set; }
    }

    public class WeeklyTrendDto
    {
        public DateTime TripDate     { get; set; }
        public int      TotalTrips   { get; set; }
        public int      DelayedCount { get; set; }
        public double   AvgDelayMins { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  GENERIC API RESPONSE WRAPPER
    // ══════════════════════════════════════════════════════════
    public class ApiResponse<T>
    {
        public bool   Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T?     Data    { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
            => new() { Success = true, Message = message, Data = data };

        public static ApiResponse<T> Fail(string message)
            => new() { Success = false, Message = message };
    }
}
