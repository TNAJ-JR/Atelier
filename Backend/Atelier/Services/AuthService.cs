using Atelier.Common;
using Atelier.Data;
using Atelier.Modeles;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Services;

public class AuthService(
    AtelierDbContext dbContext, UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IConfiguration configuration) : IAuthService
{
    private const string InvalidCredentialsMessage = "Identifiants incorrects ou compte temporairement indisponible.";
    private static readonly string DummyPasswordHash = new PasswordHasher<ApplicationUser>()
        .HashPassword(new ApplicationUser(), Guid.NewGuid().ToString());

    public async Task<ServiceResult<LoginViewModel>> LoginAsync(string email, string password)
    {
        var applicationUser = await userManager.FindByEmailAsync(email.Trim());
        if (applicationUser is null)
        {
            _ = userManager.PasswordHasher.VerifyHashedPassword(new ApplicationUser(), DummyPasswordHash, password);
            return ServiceResult<LoginViewModel>.Fail(InvalidCredentialsMessage, ServiceErrorType.Unauthorized);
        }
        var signInResult = await signInManager.CheckPasswordSignInAsync(applicationUser, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
            return ServiceResult<LoginViewModel>.Fail(InvalidCredentialsMessage, ServiceErrorType.Unauthorized);
        var refreshTokenValue = jwtService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = applicationUser.Id, TokenHash = jwtService.HashRefreshToken(refreshTokenValue), SecurityStamp = applicationUser.SecurityStamp!,
            Family = new RefreshTokenFamily(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(configuration.GetValue("Jwt:RefreshTokenExpirationDays", 14))
        };
        dbContext.RefreshTokens.Add(refreshTokenEntity);
        await dbContext.SaveChangesAsync();
        return ServiceResult<LoginViewModel>.Ok(await CreateLoginResponseAsync(applicationUser, refreshTokenEntity, refreshTokenValue), "Authentification réussie.");
    }

    public async Task<ServiceResult<LoginViewModel>> RefreshAsync(string refreshToken)
    {
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);
        var refreshTokenFamilyId = await dbContext.RefreshTokens.AsNoTracking()
            .Where(refreshTokenRecord => refreshTokenRecord.TokenHash == refreshTokenHash).Select(refreshTokenRecord => (Guid?)refreshTokenRecord.FamilyId).SingleOrDefaultAsync();
        if (refreshTokenFamilyId is null) return InvalidSession();

        await using var databaseTransaction = await dbContext.Database.BeginTransactionAsync();
        // Serialize refresh, replay detection and logout on the same refreshTokenFamily, including new descendants.
        var refreshTokenFamily = await LockRefreshTokenFamilyAsync(refreshTokenFamilyId.Value);
        if (refreshTokenFamily is null || refreshTokenFamily.RevokedAt is not null) return InvalidSession();

        var currentRefreshToken = await dbContext.RefreshTokens.Include(refreshTokenRecord => refreshTokenRecord.User)
            .SingleOrDefaultAsync(refreshTokenRecord => refreshTokenRecord.TokenHash == refreshTokenHash);
        if (currentRefreshToken is null) return InvalidSession();

        // Check reuse even if the old token has expired: its active descendants must also be revoked.
        if (currentRefreshToken.UsedAt is not null || currentRefreshToken.RevokedAt is not null)
        {
            refreshTokenFamily.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
            await databaseTransaction.CommitAsync();
            return InvalidSession();
        }
        if (currentRefreshToken.ExpiresAt <= DateTimeOffset.UtcNow ||
            currentRefreshToken.SecurityStamp != currentRefreshToken.User.SecurityStamp || await userManager.IsLockedOutAsync(currentRefreshToken.User))
        {
            refreshTokenFamily.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
            await databaseTransaction.CommitAsync();
            return InvalidSession();
        }

        var refreshTokenValue = jwtService.GenerateRefreshToken();
        var replacementRefreshToken = new RefreshToken
        {
            UserId = currentRefreshToken.UserId, TokenHash = jwtService.HashRefreshToken(refreshTokenValue),
            SecurityStamp = currentRefreshToken.SecurityStamp, FamilyId = currentRefreshToken.FamilyId,
            ExpiresAt = currentRefreshToken.ExpiresAt
        };
        currentRefreshToken.UsedAt = DateTimeOffset.UtcNow;
        currentRefreshToken.ReplacedByToken = replacementRefreshToken;
        dbContext.RefreshTokens.Add(replacementRefreshToken);
        // SaveChanges creates the successor and records consumption/link atomically in this databaseTransaction.
        await dbContext.SaveChangesAsync();
        var loginResponse = await CreateLoginResponseAsync(currentRefreshToken.User, replacementRefreshToken, refreshTokenValue);
        await databaseTransaction.CommitAsync();
        return ServiceResult<LoginViewModel>.Ok(loginResponse);
    }

    private static ServiceResult<LoginViewModel> InvalidSession() =>
        ServiceResult<LoginViewModel>.Fail("Session expirée ou invalide.", ServiceErrorType.Unauthorized);

    private Task<RefreshTokenFamily?> LockRefreshTokenFamilyAsync(Guid refreshTokenFamilyId) =>
        dbContext.Set<RefreshTokenFamily>()
            .FromSqlInterpolated($"SELECT * FROM [RefreshTokenFamilies] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {refreshTokenFamilyId}")
            .SingleOrDefaultAsync();

    private async Task<LoginViewModel> CreateLoginResponseAsync(ApplicationUser applicationUser, RefreshToken refreshTokenEntity, string refreshTokenValue)
    {
        var userRoles = await userManager.GetRolesAsync(applicationUser);
        var accessTokenResult = jwtService.GenerateAccessToken(applicationUser, refreshTokenEntity, userRoles);
        return new()
        {
            Token = accessTokenResult.Token, RefreshToken = refreshTokenValue, RefreshTokenExpiresAt = refreshTokenEntity.ExpiresAt, ExpiresAt = accessTokenResult.ExpiresAt,
            User = new(applicationUser.Id.ToString(), $"{applicationUser.Prenom} {applicationUser.Nom}".Trim(),
                applicationUser.Email!, applicationUser.Nom, applicationUser.Prenom, userRoles)
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var refreshTokenHash = jwtService.HashRefreshToken(refreshToken);
        var refreshTokenFamilyId = await dbContext.RefreshTokens.AsNoTracking()
            .Where(refreshTokenRecord => refreshTokenRecord.TokenHash == refreshTokenHash)
            .Select(refreshTokenRecord => (Guid?)refreshTokenRecord.FamilyId).SingleOrDefaultAsync();
        if (refreshTokenFamilyId is null) return;
        await using var databaseTransaction = await dbContext.Database.BeginTransactionAsync();
        var refreshTokenFamily = await LockRefreshTokenFamilyAsync(refreshTokenFamilyId.Value);
        if (refreshTokenFamily is not null && refreshTokenFamily.RevokedAt is null)
        {
            refreshTokenFamily.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }
        await databaseTransaction.CommitAsync();
    }
}
