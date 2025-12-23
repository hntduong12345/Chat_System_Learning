using ChatWebSocketAPI.DTOs;

namespace ChatWebSocketAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<UserDto?> RegisterAsync(RegisterRequest request);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAdminUsersAsync();
        string GenerateJwtToken(UserDto user);
        Guid? ValidateJwtToken(string token);
    }
}
