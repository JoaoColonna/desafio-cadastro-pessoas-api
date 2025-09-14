/*using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Infrasctructure.Database;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RegisterAPI.Tests.Integration
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove o banco real e usa InMemory para testes
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase($"InMemoryDbForTesting_{Guid.NewGuid()}"));
                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkWithToken()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "integrationuser",
                Email = "integration@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.Username.Should().Be("integrationuser");
            authResponse.Email.Should().Be("integration@example.com");
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange - Primeiro registrar um usuário
            var registerDto = new RegisterDto
            {
                Username = "loginuser",
                Email = "login@example.com",
                Password = "password123"
            };

            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto
            {
                Username = "loginuser",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponseDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.Username.Should().Be("loginuser");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "nonexistent",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturnConflict()
        {
            // Arrange - Primeiro registrar um usuário
            var firstUser = new RegisterDto
            {
                Username = "duplicateuser",
                Email = "first@example.com",
                Password = "password123"
            };

            await _client.PostAsJsonAsync("/api/auth/register", firstUser);

            var duplicateUser = new RegisterDto
            {
                Username = "duplicateuser", // Mesmo username
                Email = "second@example.com",
                Password = "password456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", duplicateUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidRegisterDto = new RegisterDto
            {
                Username = "", // Username vazio
                Email = "invalid-email", // Email inválido
                Password = "123" // Password muito curto
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRegisterDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}*/