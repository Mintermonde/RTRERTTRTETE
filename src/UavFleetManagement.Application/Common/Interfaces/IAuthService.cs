using UavFleetManagement.Application.DTOs.Auth;

namespace UavFleetManagement.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginUserAsync(LoginRequestDto request);
    Task<AuthResponseDto> RegisterSuperuserAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginSuperuserAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
}
