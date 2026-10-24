using Atelier.Common;
using Atelier.Data;
using Atelier.Modeles;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Services;

public class AuthService(
    AtelierDbContext context, UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, IJwtService jwt, IConfiguration configuration) : IAuthService
{
    private const string InvalidCredentials = "Identifiants incorrects ou compte temporairement indisponible.";
    private static readonly string DummyHash = new PasswordHasher<ApplicationUser>()
        .HashPassword(new ApplicationUser(), Guid.NewGuid().ToString());

    public async Task<ServiceResult<LoginViewModel>> LoginAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            _ = userManager.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), DummyHash, password);
            return ServiceResult<LoginViewModel>.Fail(InvalidCredentials, ServiceErrorType.Unauthorized);
        }
        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return ServiceResult<LoginViewModel>.Fail(InvalidCredentials, ServiceErrorType.Unauthorized);
        var raw = jwt.GenerateRefreshToken();
        var session = new RefreshToken
        {
            UserId = user.Id, TokenHash = jwt.HashRefreshToken(raw), SecurityStamp = user.SecurityStamp!,
            Family = new RefreshTokenFamily(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenExpirationDays", 14))
        };
        context.RefreshTokens.Add(session);
        await context.SaveChangesAsync();
        return ServiceResult<LoginViewModel>.Ok(await CreateResponse(user, session, raw), "Authentification réussie.");
    }

    public async Task<ServiceResult<LoginViewModel>> RefreshAsync(string refreshToken)
    {
        var hash = jwt.HashRefreshToken(refreshToken);
        var familyId = await context.RefreshTokens.AsNoTracking()
            .Where(x => x.TokenHash == hash).Select(x => (Guid?)x.FamilyId).SingleOrDefaultAsync();
        if (familyId is null) return InvalidSession();

        await using var transaction = await context.Database.BeginTransactionAsync();
        // Serialize refresh, replay detection and logout on the same family, including new descendants.
        var family = await LockFamily(familyId.Value);
        if (family is null || family.RevokedAt is not null) return InvalidSession();

        var current = await context.RefreshTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == hash);
        if (current is null) return InvalidSession();

        // Check reuse even if the old token has expired: its active descendants must also be revoked.
        if (current.UsedAt is not null || current.RevokedAt is not null)
        {
            family.RevokedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return InvalidSession();
        }
        if (current.ExpiresAt <= DateTimeOffset.UtcNow ||
            current.SecurityStamp != current.User.SecurityStamp || await userManager.IsLockedOutAsync(current.User))
        {
            family.RevokedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return InvalidSession();
        }

        var raw = jwt.GenerateRefreshToken();
        var replacement = new RefreshToken
        {
            UserId = current.UserId, TokenHash = jwt.HashRefreshToken(raw),
            SecurityStamp = current.SecurityStamp, FamilyId = current.FamilyId,
            ExpiresAt = current.ExpiresAt
        };
        current.UsedAt = DateTimeOffset.UtcNow;
        current.ReplacedByToken = replacement;
        context.RefreshTokens.Add(replacement);
        // SaveChanges creates the successor and records consumption/link atomically in this transaction.
        await context.SaveChangesAsync();
        var response = await CreateResponse(current.User, replacement, raw);
        await transaction.CommitAsync();
        return ServiceResult<LoginViewModel>.Ok(response);
    }

    private static ServiceResult<LoginViewModel> InvalidSession() =>
        ServiceResult<LoginViewModel>.Fail("Session expirée ou invalide.", ServiceErrorType.Unauthorized);

    private Task<RefreshTokenFamily?> LockFamily(Guid id) =>
        context.Set<RefreshTokenFamily>()
            .FromSqlInterpolated($"SELECT * FROM [RefreshTokenFamilies] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}")
            .SingleOrDefaultAsync();

    private async Task<LoginViewModel> CreateResponse(ApplicationUser user, RefreshToken session, string raw)
    {
        var roles = await userManager.GetRolesAsync(user);
        var access = jwt.GenerateAccessToken(user, session, roles);
        return new()
        {
            Token = access.Token, RefreshToken = raw, ExpiresAt = access.ExpiresAt,
            User = new(user.Id.ToString(), $"{user.Prenom} {user.Nom}".Trim(),
                user.Email!, user.Nom, user.Prenom, roles)
        };
    }

    public async Task LogoutAsync(int userId, int sessionId)
    {
        var familyId = await context.RefreshTokens.AsNoTracking()
            .Where(x => x.Id == sessionId && x.UserId == userId)
            .Select(x => (Guid?)x.FamilyId).SingleOrDefaultAsync();
        if (familyId is null) return;
        await using var transaction = await context.Database.BeginTransactionAsync();
        var family = await LockFamily(familyId.Value);
        if (family is not null && family.RevokedAt is null)
        {
            family.RevokedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync();
        }
        await transaction.CommitAsync();
    }
}
