namespace Atelier.Security;

public static class CookieAuth
{
    public const string RefreshCookieName = "__Host-atelier-refresh";
    public const string CsrfCookieName = "__Host-atelier-csrf";
    public const string CsrfHeaderName = "X-CSRF-TOKEN";

    // Cookie lifecycle endpoints deliberately do not authenticate using a bearer token.
    // This gives the antiforgery pair a stable anonymous identity, even after JWT renewal.
    public static bool IsCookieEndpoint(PathString requestPath) =>
        new[] { "/api/auth/csrf", "/api/auth/login", "/api/auth/refresh", "/api/auth/logout" }
            .Any(endpointPath => string.Equals(requestPath.Value?.TrimEnd('/'), endpointPath, StringComparison.OrdinalIgnoreCase));

    public static SameSiteMode GetSameSiteMode(IConfiguration configuration, IWebHostEnvironment hostingEnvironment) =>
        configuration.GetValue<SameSiteMode?>("AuthCookies:SameSite")
        ?? (hostingEnvironment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Strict);

    public static CookieOptions CreateCookieOptions(IConfiguration configuration, IWebHostEnvironment hostingEnvironment,
        DateTimeOffset? expiresAt = null) => new()
        {
            HttpOnly = true, Secure = true, Path = "/",
            SameSite = GetSameSiteMode(configuration, hostingEnvironment), Expires = expiresAt,
            IsEssential = true
        };
}
