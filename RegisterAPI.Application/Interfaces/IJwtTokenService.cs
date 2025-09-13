namespace RegisterAPI.Application.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(string username, string email, int userId);
    }
}