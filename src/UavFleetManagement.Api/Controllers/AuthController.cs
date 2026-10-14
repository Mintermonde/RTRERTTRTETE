using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UavFleetManagement.Application.Common.Interfaces;
using UavFleetManagement.Application.DTOs.Auth;

namespace UavFleetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Авторизация пользователя (клиент, пилот, инженер, агент, бухгалтер, аудитор)
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginUserAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterUserAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Вход для суперпользователя (Администратора)
    /// </summary>
    [HttpPost("superuser/login")]
    [AllowAnonymous]
    public async Task<IActionResult> SuperuserLogin([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginSuperuserAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Регистрация нового суперпользователя (доступна только действующим суперпользователям)
    /// </summary>
    [HttpPost("superuser/register")]
    [Authorize(Roles = "Superuser")]
    public async Task<IActionResult> SuperuserRegister([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterSuperuserAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Обновление JWT токена по Refresh Token (каждые 30 минут)
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        return Ok(result);
    }
}
