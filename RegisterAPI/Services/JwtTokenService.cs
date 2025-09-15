using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegisterAPI.Application.Interfaces;

namespace RegisterAPI.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public JwtTokenService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public string GenerateToken(string username, string email, int userId)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            
            // Usar a mesma lógica do Program.cs para buscar configurações
            var secretKey = _environment.IsProduction() 
                ? Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT SecretKey não configurada em produção")
                : jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");

            var issuer = _environment.IsProduction()
                ? Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "RegisterAPI"
                : jwtSettings["Issuer"] ?? "RegisterAPI";

            var audience = _environment.IsProduction()
                ? Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "RegisterAPI"
                : jwtSettings["Audience"] ?? "RegisterAPI";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}