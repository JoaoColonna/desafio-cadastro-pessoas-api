using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Controllers;
using Xunit;

namespace RegisterAPI.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IJwtTokenService> _mockJwtTokenService;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockJwtTokenService = new Mock<IJwtTokenService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _controller = new AuthController(_mockAuthService.Object, _mockJwtTokenService.Object);

            // Setup HttpContext para poder injetar o UserRepository
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IUserRepository))).Returns(_mockUserRepository.Object);
            
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };
            
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "password123"
            };

            var authResponse = new AuthResponseDto
            {
                Username = "testuser",
                Email = "test@example.com",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Token = string.Empty
            };

            var user = new RegisterAPI.Domain.Entities.User
            {
                Id = 1,
                Username = "testuser",
                Email = "test@example.com"
            };

            var token = "jwt-token-123";

            _mockAuthService.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(authResponse);
            _mockUserRepository.Setup(x => x.GetByUsernameAsync(loginDto.Username)).ReturnsAsync(user);
            _mockJwtTokenService.Setup(x => x.GenerateToken(user.Username, user.Email, user.Id)).Returns(token);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as AuthResponseDto;
            response.Should().NotBeNull();
            response!.Token.Should().Be(token);
            response.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task Login_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Username é obrigatório");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_WithValidData_ShouldReturnOkWithToken()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var authResponse = new AuthResponseDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Token = string.Empty
            };

            var user = new RegisterAPI.Domain.Entities.User
            {
                Id = 1,
                Username = "newuser",
                Email = "newuser@example.com"
            };

            var token = "jwt-token-456";

            _mockAuthService.Setup(x => x.RegisterAsync(registerDto)).ReturnsAsync(authResponse);
            _mockUserRepository.Setup(x => x.GetByUsernameAsync(registerDto.Username)).ReturnsAsync(user);
            _mockJwtTokenService.Setup(x => x.GenerateToken(user.Username, user.Email, user.Id)).Returns(token);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as AuthResponseDto;
            response.Should().NotBeNull();
            response!.Token.Should().Be(token);
            response.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task Register_WithInvalidModelState_ShouldReturnBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "", Email = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Username é obrigatório");

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}