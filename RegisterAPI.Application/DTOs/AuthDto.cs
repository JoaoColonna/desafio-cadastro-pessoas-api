using System.ComponentModel.DataAnnotations;

namespace RegisterAPI.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Username � obrigat�rio")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password � obrigat�rio")]
        public string Password { get; set; } = null!;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "Username � obrigat�rio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username deve ter entre 3 e 50 caracteres")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Email � obrigat�rio")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato v�lido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password � obrigat�rio")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter pelo menos 6 caracteres")]
        public string Password { get; set; } = null!;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}