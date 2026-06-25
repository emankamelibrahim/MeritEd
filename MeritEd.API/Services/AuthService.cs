using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public AuthService(UserManager<User> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<(bool Success, string[] Errors, User? User)> RegisterAsync(
        string email, string password, string displayName, UserRole role)
    {
        if (role == UserRole.Admin)
        {
            return (false, new[] { "Cannot self-register as Admin." }, null);
        }

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (false, new[] { "Email is already registered." }, null);
        }

        var user = new User
        {
            Email = email,
            UserName = email,
            DisplayName = displayName,
            Role = role,
            IsApproved = role == UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors, null);
        }

        return (true, Array.Empty<string>(), user);
    }

    public async Task<(bool Success, string Error, User? User)> ValidateCredentialsAsync(
        string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (false, "Invalid email or password.", null);
        }

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isValid)
        {
            return (false, "Invalid email or password.", null);
        }

        return (true, string.Empty, user);
    }

    public async Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Valid, User? User)> ValidateRefreshTokenAsync(string token)
    {
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return (false, null);
        }

        return (true, storedToken.User);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken != null)
        {
            storedToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }
}