using Atelier.Modeles;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Atelier.Services;

public class JwtService(IConfiguration configuration) : IJwtService
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(
        ApplicationUser user, RefreshToken session, IList<string> roles)
    {
        var expiration = DateTimeOffset.UtcNow.AddMinutes(configuration.GetValue("Jwt:ExpirationMinutes", 15));
        if (expiration > session.ExpiresAt) expiration = session.ExpiresAt;
        var claims = new List<Claim>
        {
            new("sub", user.Id.ToString()), new("sid", session.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()), new("stamp", session.SecurityStamp),
            new("name", $"{user.Prenom} {user.Nom}".Trim()), new("email", user.Email!)
        };
        claims.AddRange(roles.Select(role => new Claim("role", role)));
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Convert.FromBase64String(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
            claims, DateTime.UtcNow, expiration.UtcDateTime, credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    public string HashRefreshToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
