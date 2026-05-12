using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SistemaHotel.Server.Utilidades
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);
        int GetTokenExpirationMinutes();
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly int _expirationMinutes = 60;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            var expConfig = _configuration["Jwt:ExpirationMinutes"];
            if (int.TryParse(expConfig, out int exp))
            {
                _expirationMinutes = exp;
            }
        }

        public string GenerateToken(int userId, string email, string role)
        {
            var jwtSecret = _configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
                throw new InvalidOperationException("JWT Secret no configurado en appsettings.json");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "SistemaHotel",
                audience: _configuration["Jwt:Audience"] ?? "SistemaHotelClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int GetTokenExpirationMinutes()
        {
            return _expirationMinutes;
        }
    }
}
