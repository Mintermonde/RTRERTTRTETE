namespace UavFleetManagement.Application.DTOs.Auth;

public record LoginRequestDto(string Username, string Password);

public record RegisterRequestDto(
    string Username,
    string Email,
    string Password,
    int? LinkedUserId = null,
    int? LinkedEngineerId = null,
    string? Role = "Client"
);

public record RefreshTokenRequestDto(
    string AccessToken,
    string RefreshToken
);

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    string Username,
    string Role,
    DateTime ExpiresAt,
    int ExpiresInMinutes = 30
);
