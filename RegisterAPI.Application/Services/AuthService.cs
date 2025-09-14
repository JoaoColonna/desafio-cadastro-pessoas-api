using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;
using RegisterAPI.Domain.Entities;
using System.Security.Cryptography;

namespace RegisterAPI.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Username ou password inválidos.");
            }

            // O token será gerado no controller usando o serviço JWT
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                Token = string.Empty, // Será preenchido no controller
                Username = user.Username,
                Email = user.Email,
                ExpiresAt = expiresAt
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Verificar se username já existe
            if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
            {
                throw new InvalidOperationException("Username já está em uso.");
            }

            // Verificar se email já existe
            if (await _userRepository.ExistsByEmailAsync(registerDto.Email))
            {
                throw new InvalidOperationException("Email já está em uso.");
            }

            // Criar hash da senha
            var passwordHash = HashPassword(registerDto.Password);

            // Criar usuário
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // O token será gerado no controller
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return new AuthResponseDto
            {
                Token = string.Empty, // Será preenchido no controller
                Username = createdUser.Username,
                Email = createdUser.Email,
                ExpiresAt = expiresAt
            };
        }

        public string GenerateJwtToken(string username, string email, int userId)
        {
            // Este método será implementado usando injeção de dependência no controller
            throw new NotImplementedException("Use IJwtTokenService para gerar tokens");
        }

        private static string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashBytes = Convert.FromBase64String(hashedPassword);
            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
    }
}