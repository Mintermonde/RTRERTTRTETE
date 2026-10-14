using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UavFleetManagement.Application.Common.Exceptions;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Auth;
using UavFleetManagement.Domain.Entities.Identity;

namespace UavFleetManagement.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public AuthService(IApplicationDbContext context, IPasswordHasher hasher, ITokenService tokenService)
    {
        _context = context;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task<AuthResponseDto> RegisterUserAsync(RegisterRequestDto request)
    {
        if (await _context.UserCredentials.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Пользователь с таким именем уже существует.");
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);
        var user = new UserCredential
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            LinkedUserId = request.LinkedUserId,
            LinkedEngineerId = request.LinkedEngineerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.UserCredentials.Add(user);
        await _context.SaveChangesAsync();

        string role = !string.IsNullOrWhiteSpace(request.Role) ? request.Role : "User";
        return await CreateAuthSessionAsync(user.UserCredentialId, user.Username, role, isSuperuser: false);
    }

    public async Task<AuthResponseDto> LoginUserAsync(LoginRequestDto request)
    {
        var user = await _context.UserCredentials
            .Include(u => u.LinkedEngineer)
            .Include(u => u.LinkedUser)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !_hasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedException("Неверный логин или пароль.");
        }

        if (user.IsBlocked)
        {
            throw new ForbiddenException("Учетная запись заблокирована.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Определение роли пользователя
        string role = "Client";
        if (user.LinkedEngineerId.HasValue) role = "Engineer";
        else if (user.LinkedUserId.HasValue) role = "Pilot";
        else if (user.Username.Contains("agent", StringComparison.OrdinalIgnoreCase)) role = "Agent";
        else if (user.Username.Contains("accountant", StringComparison.OrdinalIgnoreCase)) role = "Accountant";
        else if (user.Username.Contains("auditor", StringComparison.OrdinalIgnoreCase)) role = "Auditor";

        return await CreateAuthSessionAsync(user.UserCredentialId, user.Username, role, isSuperuser: false);
    }

    public async Task<AuthResponseDto> RegisterSuperuserAsync(RegisterRequestDto request)
    {
        if (await _context.SuperuserCredentials.AnyAsync(u => u.Username == request.Username))
        {
            throw new BadRequestException("Суперпользователь с таким именем уже существует.");
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);
        var superuser = new SuperuserCredential
        {
            Username = request.Username,
            PasswordHash = hash,
            PasswordSalt = salt,
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.SuperuserCredentials.Add(superuser);
        await _context.SaveChangesAsync();

        return await CreateAuthSessionAsync(superuser.SuperuserId, superuser.Username, "Superuser", isSuperuser: true);
    }

    public async Task<AuthResponseDto> LoginSuperuserAsync(LoginRequestDto request)
    {
        var superuser = await _context.SuperuserCredentials.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (superuser == null || !_hasher.VerifyPassword(request.Password, superuser.PasswordHash, superuser.PasswordSalt))
        {
            throw new UnauthorizedException("Неверные учетные данные администратора.");
        }

        superuser.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await CreateAuthSessionAsync(superuser.SuperuserId, superuser.Username, "Superuser", isSuperuser: true);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new UnauthorizedException("Недействительный Access Token.");
        }

        var username = principal.FindFirst(ClaimTypes.Name)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        var role = principal.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        var idStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(username) || !int.TryParse(idStr, out int userId))
        {
            throw new UnauthorizedException("Некорректная структура токена.");
        }

        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedException("Refresh Token недействителен, отозван или срок его действия истек.");
        }

        // Ротация токена: отзываем старый
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Создаем новую сессию
        bool isSuperuser = role == "Superuser" || role == "Admin";
        var newAuth = await CreateAuthSessionAsync(userId, username, role, isSuperuser);
        storedToken.ReplacedByTokenHash = HashToken(newAuth.RefreshToken);

        await _context.SaveChangesAsync();
        return newAuth;
    }

    private async Task<AuthResponseDto> CreateAuthSessionAsync(int id, string username, string role, bool isSuperuser)
    {
        // Access Token действует ровно 30 минут (требование: обновление каждые 30 минут)
        var accessToken = _tokenService.GenerateToken(id, username, role, expiryMinutes: 30);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = HashToken(rawRefreshToken);

        var userRefreshToken = new UserRefreshToken
        {
            UserId = isSuperuser ? null : id,
            SuperuserId = isSuperuser ? id : null,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _context.UserRefreshTokens.Add(userRefreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto(
            accessToken,
            rawRefreshToken,
            username,
            role,
            DateTime.UtcNow.AddMinutes(30),
            ExpiresInMinutes: 30
        );
    }
}
