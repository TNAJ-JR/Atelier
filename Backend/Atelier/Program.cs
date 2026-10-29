using Atelier.Data;
using Atelier.Modeles;
using Atelier.Security;
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
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? (builder.Environment.IsDevelopment() ? ["http://localhost:5173"] : []);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    if (allowedOrigins.Length > 0) policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById(
    builder.Configuration["Dashboard:TimeZone"] ?? "America/Toronto"));
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = CookieAuth.CsrfHeaderName;
    options.Cookie.Name = CookieAuth.CsrfCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = CookieAuth.GetSameSiteMode(builder.Configuration, builder.Environment);
    options.Cookie.Path = "/";
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<CookieCsrfFilter>();
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

var encodedJwtSigningKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is not configured.");
var jwtSigningKeyBytes = Convert.FromBase64String(encodedJwtSigningKey);
if (jwtSigningKeyBytes.Length < 32) throw new InvalidOperationException("Clé Jwt trop courte.");
foreach (var settingName in new[] { "Issuer", "Audience" })
    if (string.IsNullOrWhiteSpace(builder.Configuration[$"Jwt:{settingName}"]))
        throw new InvalidOperationException($"Jwt:{settingName} doit être configuré.");
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RequireSignedTokens = true,
        RequireExpirationTime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtSigningKeyBytes),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "name",
        RoleClaimType = "role"
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = authenticationContext =>
        {
            if (CookieAuth.IsCookieEndpoint(authenticationContext.Request.Path))
                authenticationContext.NoResult();
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var principal = context.Principal!;
            if (!int.TryParse(principal.FindFirst("sub")?.Value, out var applicationUserId) ||
                !int.TryParse(principal.FindFirst("sid")?.Value, out var refreshTokenId))
            {
                context.Fail("Session invalide.");
                return;
            }
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AtelierDbContext>();
            var refreshTokenEntity = await dbContext.RefreshTokens.AsNoTracking().Include(x => x.User).Include(x => x.Family)
                .SingleOrDefaultAsync(x => x.Id == refreshTokenId && x.UserId == applicationUserId,
                    context.HttpContext.RequestAborted);
            if (refreshTokenEntity is null || refreshTokenEntity.RevokedAt is not null || refreshTokenEntity.ExpiresAt <= DateTimeOffset.UtcNow ||
                refreshTokenEntity.Family.RevokedAt is not null ||
                refreshTokenEntity.SecurityStamp != refreshTokenEntity.User.SecurityStamp ||
                refreshTokenEntity.SecurityStamp != principal.FindFirst("stamp")?.Value)
            {
                context.Fail("Session expirée ou révoquée.");
                return;
            }
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var currentUserRoles = await userManager.GetRolesAsync(refreshTokenEntity.User);
            var jwtRoles = principal.FindAll("role").Select(x => x.Value);
            if (await userManager.IsLockedOutAsync(refreshTokenEntity.User) || !currentUserRoles.Order().SequenceEqual(jwtRoles.Order()))
                context.Fail("Compte verrouillé ou droits modifiés.");
        }
    };
});
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());

//Rate limiting pour les endpoints d'authentification (login et refresh) pour limiter les tentatives de brute force
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = builder.Configuration.GetValue("Auth:RequestsPerMinute", 10),
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
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
await using (var serviceScope = app.Services.CreateAsyncScope())
{
    await RolesInitializer.InitializeAsync(
        serviceScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(),
        serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>(),
        app.Configuration, app.Environment.IsDevelopment() && app.Configuration.GetValue("SeedUsers:Enabled", false));
}
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
