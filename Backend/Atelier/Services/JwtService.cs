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
        ApplicationUser applicationUser, RefreshToken refreshTokenEntity, IList<string> userRoles)
    {
        var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(configuration.GetValue("Jwt:ExpirationMinutes", 15));
        if (accessTokenExpiresAt > refreshTokenEntity.ExpiresAt) accessTokenExpiresAt = refreshTokenEntity.ExpiresAt;
        var jwtClaims = new List<Claim>
        {
            new("sub", applicationUser.Id.ToString()), new("sid", refreshTokenEntity.Id.ToString()),
            new("jti", Guid.NewGuid().ToString()), new("stamp", refreshTokenEntity.SecurityStamp),
            new("name", $"{applicationUser.Prenom} {applicationUser.Nom}".Trim()), new("email", applicationUser.Email!)
        };
        jwtClaims.AddRange(userRoles.Select(roleName => new Claim("role", roleName)));
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Convert.FromBase64String(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);
        var accessToken = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
            jwtClaims, DateTime.UtcNow, accessTokenExpiresAt.UtcDateTime, signingCredentials);
        return (new JwtSecurityTokenHandler().WriteToken(accessToken), accessTokenExpiresAt);
    }

    public string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    public string HashRefreshToken(string refreshTokenValue) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshTokenValue)));
}
