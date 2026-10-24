using Atelier.Modeles;
namespace Atelier.Services;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(ApplicationUser applicationUser, RefreshToken refreshTokenEntity, IList<string> userRoles);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshTokenValue);
}
