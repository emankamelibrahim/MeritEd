using MeritEd.Core.Entities;
using MeritEd.Core.Enums;

namespace MeritEd.Core.Interfaces.Services;

public interface IAuthService
{
    Task<(bool Success, string[] Errors, User? User)> RegisterAsync(
        string email, string password, string displayName, UserRole role);

    Task<(bool Success, string Error, User? User)> ValidateCredentialsAsync(
        string email, string password);

    Task SaveRefreshTokenAsync(Guid userId, string token, DateTime expiresAt);

    Task<(bool Valid, User? User)> ValidateRefreshTokenAsync(string token);

    Task RevokeRefreshTokenAsync(string token);
}