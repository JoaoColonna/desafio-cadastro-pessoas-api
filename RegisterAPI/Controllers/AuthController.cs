using Microsoft.AspNetCore.Mvc;
using RegisterAPI.Application.DTOs;
using RegisterAPI.Application.Interfaces;

namespace RegisterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(IAuthService authService, IJwtTokenService jwtTokenService)
        {
            _authService = authService;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Realiza login do usu�rio
        /// </summary>
        /// <param name="loginDto">Dados de login</param>
        /// <returns>Token JWT e informa��es do usu�rio</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.LoginAsync(loginDto);
            
            // Gerar token JWT
            // Precisamos buscar o userId - vamos buscar novamente do banco
            // Em uma implementa��o mais otimizada, retornar�amos o User completo do AuthService
            var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetByUsernameAsync(loginDto.Username);
            
            if (user != null)
            {
                response.Token = _jwtTokenService.GenerateToken(user.Username, user.Email, user.Id);
            }

            return Ok(response);
        }

        /// <summary>
        /// Registra um novo usu�rio
        /// </summary>
        /// <param name="registerDto">Dados de registro</param>
        /// <returns>Token JWT e informa��es do usu�rio</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.RegisterAsync(registerDto);
            
            // Gerar token JWT
            var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await userRepository.GetByUsernameAsync(registerDto.Username);
            
            if (user != null)
            {
                response.Token = _jwtTokenService.GenerateToken(user.Username, user.Email, user.Id);
            }

            return Ok(response);
        }
    }
}