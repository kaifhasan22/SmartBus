using Microsoft.IdentityModel.Tokens;
using SmartBusAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartBusAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry      = DateTime.UtcNow.AddHours(int.Parse(jwtSettings["ExpiryHours"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role,               user.Role),
                new Claim("name",                        user.Name),
                new Claim("userId",                      user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             jwtSettings["Issuer"],
                audience:           jwtSettings["Audience"],
                claims:             claims,
                expires:            expiry,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int GetUserIdFromToken(ClaimsPrincipal user)
        {
            var claim = user.FindFirst("userId") ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
            return int.TryParse(claim?.Value, out var id) ? id : 0;
        }
    }
}
