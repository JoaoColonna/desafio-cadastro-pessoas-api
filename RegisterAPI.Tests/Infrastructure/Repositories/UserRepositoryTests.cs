using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RegisterAPI.Domain.Entities;
using RegisterAPI.Infrasctructure.Database;
using RegisterAPI.Infrasctructure.Repositories;
using Xunit;

namespace RegisterAPI.Tests.Infrastructure.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _repository = new UserRepository(_context);
        }

        [Fact]
        public async Task CreateAsync_WithValidUser_ShouldReturnCreatedUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true
            };

            // Act
            var result = await _repository.CreateAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Username.Should().Be(user.Username);
            result.Email.Should().Be(user.Email);
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task GetByUsernameAsync_WithExistingUsername_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUsernameAsync("existinguser");

            // Assert
            result.Should().NotBeNull();
            result!.Username.Should().Be("existinguser");
            result.Email.Should().Be("existing@example.com");
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetByUsernameAsync_WithNonExistentUsername_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByUsernameAsync("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByUsernameAsync_WithInactiveUser_ShouldReturnNull()
        {
            // Arrange
            var user = new User
            {
                Username = "inactiveuser",
                Email = "inactive@example.com",
                PasswordHash = "hashedpassword",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByUsernameAsync("inactiveuser");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByEmailAsync("test@example.com");

            // Assert
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(user.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(user.Id);
            result.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task ExistsByUsernameAsync_WithExistingUsername_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "existinguser",
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsByUsernameAsync("existinguser");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByUsernameAsync_WithNonExistentUsername_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByUsernameAsync("nonexistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Email = "existing@example.com",
                PasswordHash = "hashedpassword",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsByEmailAsync("existing@example.com");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.ExistsByEmailAsync("nonexistent@example.com");

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}