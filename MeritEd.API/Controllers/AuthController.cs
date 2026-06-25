using MeritEd.API.DTOs.Auth;
using MeritEd.API.Services;
using MeritEd.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, ITokenService tokenService, IConfiguration configuration)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(MeritEd.API.DTOs.Auth.RegisterRequest request){
        var (success, errors, user) = await _authService.RegisterAsync(
            request.Email, request.Password, request.DisplayName, request.Role);

        if (!success)
        {
            return BadRequest(new { status = 400, error = "Registration failed", details = errors });
        }

        return Ok(new
        {
            message = "Registration successful.",
            userId = user!.Id,
            email = user.Email,
            isApproved = user.IsApproved
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var (success, error, user) = await _authService.ValidateCredentialsAsync(
            request.Email, request.Password);

        if (!success)
        {
            return Unauthorized(new { status = 401, error });
        }

        var accessToken = _tokenService.GenerateAccessToken(user!);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays);

        await _authService.SaveRefreshTokenAsync(user!.Id, refreshToken, refreshExpiresAt);

        var accessExpiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!);

        return Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            IsApproved = user.IsApproved
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(MeritEd.API.DTOs.Auth.RefreshRequest request)
    {
        var (valid, user) = await _authService.ValidateRefreshTokenAsync(request.RefreshToken);

        if (!valid)
        {
            return Unauthorized(new { status = 401, error = "Invalid or expired refresh token." });
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user!);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(refreshExpiryDays);

        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        await _authService.SaveRefreshTokenAsync(user!.Id, newRefreshToken, refreshExpiresAt);

        var accessExpiryMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!);

        return Ok(new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(accessExpiryMinutes),
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            IsApproved = user.IsApproved
        });
    }
}