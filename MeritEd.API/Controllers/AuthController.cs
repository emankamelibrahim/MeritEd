using MeritEd.API.DTOs.Auth;
using MeritEd.API.Services;
using MeritEd.Core.Entities;
using MeritEd.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";

    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, ITokenService tokenService,
        IConfiguration configuration)
    {
        _authService = authService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    private void SetRefreshCookie(string token, DateTime expiresAt) =>
        Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            Path = "/api/auth"
        });

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/api/auth"
        });

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpiresAt = DateTime.UtcNow.AddDays(
            int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!));

        await _authService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshExpiresAt);
        SetRefreshCookie(refreshToken, refreshExpiresAt);

        return new AuthResponse
        {
            AccessToken = accessToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!)),
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.DisplayName,
            Role = user.Role.ToString(),
            IsApproved = user.IsApproved
        };
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(MeritEd.API.DTOs.Auth.RegisterRequest request)
    {
        var (success, errors, user) = await _authService.RegisterAsync(
            request.Email, request.Password, request.DisplayName, request.Role);

        if (!success)
            return BadRequest(new { status = 400, error = "Registration failed", details = errors });

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
            return Unauthorized(new { status = 401, error });

        return Ok(await IssueTokensAsync(user!));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var token = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(token))
            return Unauthorized(new { status = 401, error = "Refresh token missing." });

        var (valid, user) = await _authService.ValidateRefreshTokenAsync(token);
        if (!valid)
        {
            ClearRefreshCookie();
            return Unauthorized(new { status = 401, error = "Invalid or expired refresh token." });
        }

        await _authService.RevokeRefreshTokenAsync(token);
        return Ok(await IssueTokensAsync(user!));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(token))
            await _authService.RevokeRefreshTokenAsync(token);

        ClearRefreshCookie();
        return Ok(new { message = "Logged out." });
    }
}