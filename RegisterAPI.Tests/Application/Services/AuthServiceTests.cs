using FluentAssertions;
using Moq;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Application.Services;
using RegisterAPI.Domain.Entities;
using Xunit;

namespace RegisterAPI.Tests.Application.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _authService = new AuthService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            };

            var hashedPassword = HashPassword("password123");
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("testuser");
            result.Email.Should().Be("test@example.com");
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            _mockUserRepository.Verify(x => x.GetByUsernameAsync(loginDto.Username), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidUsername_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "invaliduser",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync((User?)null);

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginDto))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Username ou password inválidos.");
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            var hashedPassword = HashPassword("correctpassword");
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = hashedPassword,
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username))
                .ReturnsAsync(user);

            // Act & Assert
            await _authService.Invoking(x => x.LoginAsync(loginDto))
                .Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Username ou password inválidos.");
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var createdUser = new User
            {
                Id = 1,
                Username = "newuser",
                Email = "newuser@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true
            };

            _mockUserRepository.Setup(x => x.ExistsByUsernameAsync(registerDto.Username))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.ExistsByEmailAsync(registerDto.Email))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(createdUser);

            // Act
            var result = await _authService.RegisterAsync(registerDto);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("newuser");
            result.Email.Should().Be("newuser@example.com");
            result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.ExistsByUsernameAsync(registerDto.Username))
                .ReturnsAsync(true);

            // Act & Assert
            await _authService.Invoking(x => x.RegisterAsync(registerDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Username já está em uso.");
        }

        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "existing@example.com",
                Password = "password123"
            };

            _mockUserRepository.Setup(x => x.ExistsByUsernameAsync(registerDto.Username))
                .ReturnsAsync(false);
            _mockUserRepository.Setup(x => x.ExistsByEmailAsync(registerDto.Email))
                .ReturnsAsync(true);

            // Act & Assert
            await _authService.Invoking(x => x.RegisterAsync(registerDto))
                .Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email já está em uso.");
        }

        [Fact]
        public void GenerateJwtToken_ShouldThrowNotImplementedException()
        {
            // Act & Assert
            _authService.Invoking(x => x.GenerateJwtToken("user", "email", 1))
                .Should().Throw<NotImplementedException>()
                .WithMessage("Use IJwtTokenService para gerar tokens");
        }

        // Helper method para simular hash de senha (simplificado para testes)
        private static string HashPassword(string password)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }
    }
}