using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<bool> UpdateFCMTokenAsync(int userId, string token);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtService  _jwt;

        public AuthService(AppDbContext db, JwtService jwt)
        {
            _db  = db;
            _jwt = jwt;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
            // TEMPORARY FIX: Let the API generate a real, valid password hash
            if (user != null && user.Email == "admin@college.edu")
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123");
                await _db.SaveChangesAsync();
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResponse { Success = false, Message = "Invalid email or password." };

            var token = _jwt.GenerateToken(user);

            return new AuthResponse
            {
                Success = true,
                Token   = token,
                Role    = user.Role,
                UserId  = user.Id,
                Name    = user.Name,
                Email   = user.Email,
                Message = "Login successful"
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email))
                return new AuthResponse { Success = false, Message = "Email already registered." };

            var validRoles = new[] { "Admin", "Driver", "Student" };
            if (!validRoles.Contains(request.Role))
                return new AuthResponse { Success = false, Message = "Invalid role." };

            var user = new User
            {
                Name         = request.Name,
                Email        = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role         = request.Role,
                PhoneNumber  = request.PhoneNumber,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user);

            return new AuthResponse
            {
                Success = true,
                Token   = token,
                Role    = user.Role,
                UserId  = user.Id,
                Name    = user.Name,
                Email   = user.Email,
                Message = "Registration successful"
            };
        }

        public async Task<bool> UpdateFCMTokenAsync(int userId, string token)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.FCMToken  = token;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
