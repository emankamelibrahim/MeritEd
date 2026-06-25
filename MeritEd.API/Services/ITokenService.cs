using MeritEd.Core.Entities;

namespace MeritEd.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}