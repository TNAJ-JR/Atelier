using Atelier.Data;
using Atelier.Modeles;
using Atelier.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument();
builder.Services.AddDbContext<AtelierDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? (builder.Environment.IsDevelopment() ? ["http://localhost:5173"] : []);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
}).AddEntityFrameworkStores<AtelierDbContext>().AddDefaultTokenProviders();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
var keyBytes = Convert.FromBase64String(jwtKey);
if (keyBytes.Length < 32) throw new InvalidOperationException("Clé Jwt trop courte.");
foreach (var name in new[] { "Issuer", "Audience" })
    if (string.IsNullOrWhiteSpace(builder.Configuration[$"Jwt:{name}"]))
        throw new InvalidOperationException($"Jwt:{name} doit être configuré.");
if (builder.Configuration.GetValue("Jwt:ExpirationMinutes", 15) is < 1 or > 60 ||
    builder.Configuration.GetValue("Jwt:RefreshTokenExpirationDays", 14) is < 1 or > 90)
    throw new InvalidOperationException("Durée JWT invalide (1-60 minutes, renouvellement 1-90 jours).");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
        RequireSignedTokens = true, RequireExpirationTime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.Zero, NameClaimType = "name", RoleClaimType = "role"
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal!;
            if (!int.TryParse(principal.FindFirst("sub")?.Value, out var userId) ||
                !int.TryParse(principal.FindFirst("sid")?.Value, out var sessionId))
            {
                context.Fail("Session invalide.");
                return;
            }
            var db = context.HttpContext.RequestServices.GetRequiredService<AtelierDbContext>();
            var session = await db.RefreshTokens.AsNoTracking().Include(x => x.User).Include(x => x.Family)
                .SingleOrDefaultAsync(x => x.Id == sessionId && x.UserId == userId,
                    context.HttpContext.RequestAborted);
            if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTimeOffset.UtcNow ||
                session.Family.RevokedAt is not null ||
                session.SecurityStamp != session.User.SecurityStamp ||
                session.SecurityStamp != principal.FindFirst("stamp")?.Value)
            {
                context.Fail("Session expirée ou révoquée.");
                return;
            }
            var users = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var currentRoles = await users.GetRolesAsync(session.User);
            var tokenRoles = principal.FindAll("role").Select(x => x.Value);
            if (await users.IsLockedOutAsync(session.User) || !currentRoles.Order().SequenceEqual(tokenRoles.Order()))
                context.Fail("Compte verrouillé ou droits modifiés.");
        }
    };
});
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Auth:RequestsPerMinute", 10),
            Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(options => options.Path = "/openapi/{documentName}.json");
    app.MapScalarApiReference().AllowAnonymous();
}
await using (var scope = app.Services.CreateAsyncScope())
{
    await RolesInitializer.InitializeAsync(
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>(),
        app.Configuration, app.Environment.IsDevelopment() && app.Configuration.GetValue("SeedUsers:Enabled", false));
}
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
public partial class Program { }
