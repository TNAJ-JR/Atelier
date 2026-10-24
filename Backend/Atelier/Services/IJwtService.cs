using Atelier.Modeles;
namespace Atelier.Services;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(ApplicationUser user, RefreshToken session, IList<string> roles);
    string GenerateRefreshToken();
    string HashRefreshToken(string token);
}
