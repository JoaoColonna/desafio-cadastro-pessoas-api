using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Moq;
using RegisterAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace RegisterAPI.Tests.Services
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly IConfiguration _configuration;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;

        public JwtTokenServiceTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:SecretKey", "SuperSecretKeyForJWTTokenGeneration2024!@#$%^&*()_+"},
                    {"JwtSettings:Issuer", "RegisterAPI"},
                    {"JwtSettings:Audience", "RegisterAPI"}
                });

            _configuration = configurationBuilder.Build();
            
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(x => x.EnvironmentName).Returns("Development"); // Simular desenvolvimento
            
            _jwtTokenService = new JwtTokenService(_configuration, _mockEnvironment.Object);
        }

        [Fact]
        public void GenerateToken_WithValidData_ShouldReturnValidJwtToken()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var userId = 1;

            // Act
            var token = _jwtTokenService.GenerateToken(username, email, userId);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            // Verificar se é um JWT válido
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            jsonToken.Should().NotBeNull();
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == username);
            jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
            jsonToken.Issuer.Should().Be("RegisterAPI");
            jsonToken.Audiences.Should().Contain("RegisterAPI");
        }

        [Fact]
        public void GenerateToken_WithoutSecretKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var configWithoutSecretKey = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:Issuer", "RegisterAPI"},
                    {"JwtSettings:Audience", "RegisterAPI"}
                })
                .Build();

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(x => x.EnvironmentName).Returns("Development");
            
            var service = new JwtTokenService(configWithoutSecretKey, mockEnv.Object);

            // Act & Assert
            service.Invoking(x => x.GenerateToken("user", "email@test.com", 1))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("JWT SecretKey não configurada");
        }

        [Fact]
        public void GenerateToken_ShouldCreateTokenWithExpiration()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var userId = 1;

            // Act
            var token = _jwtTokenService.GenerateToken(username, email, userId);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            jsonToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
            jsonToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void GenerateToken_ShouldIncludeJtiAndIatClaims()
        {
            // Arrange
            var username = "testuser";
            var email = "test@example.com";
            var userId = 1;

            // Act
            var token = _jwtTokenService.GenerateToken(username, email, userId);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
            jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);

            var jtiClaim = jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti);
            Guid.TryParse(jtiClaim.Value, out _).Should().BeTrue();
        }
    }
}