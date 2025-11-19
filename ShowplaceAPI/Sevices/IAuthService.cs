using ShowplaceAPI.Models.DTOModles;
using ShowplaceAPI.Models;

namespace ShowplaceAPI.Sevices
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto);
        string GenerateJwtToken(User user);
    }
}
