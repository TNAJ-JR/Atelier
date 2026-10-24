using Atelier.Common;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.Security;
using Atelier.Services;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atelier.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize]
[ServiceFilter(typeof(CookieCsrfFilter))]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthController(IAuthService authService, UserManager<ApplicationUser> userManager,
    IAntiforgery antiforgeryService, IConfiguration configuration, IWebHostEnvironment hostingEnvironment) : ControllerBase
{
    [HttpGet("csrf"), AllowAnonymous]
    public IActionResult Csrf()
    {
        var antiforgeryTokens = antiforgeryService.GetAndStoreTokens(HttpContext);
        return Ok(new { csrfToken = antiforgeryTokens.RequestToken });
    }

    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authenticationResult = await authService.LoginAsync(request.Email, request.Password);
        if (authenticationResult.IsSuccess && Request.Cookies.TryGetValue(CookieAuth.RefreshCookieName, out var previousRefreshToken))
            await authService.LogoutAsync(previousRefreshToken);
        return ToHttpResult(authenticationResult);
    }

    [HttpPost("refresh"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue(CookieAuth.RefreshCookieName, out var refreshTokenValue))
        {
            ClearRefreshCookie();
            return Unauthorized(new { message = "Session absente ou expirée." });
        }
        var authenticationResult = await authService.RefreshAsync(refreshTokenValue);
        if (!authenticationResult.IsSuccess) ClearRefreshCookie();
        return ToHttpResult(authenticationResult);
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var applicationUser = await userManager.FindByIdAsync(User.FindFirst("sub")!.Value);
        if (applicationUser is null) return Unauthorized(new { message = "Session invalide." });
        return Ok(new UserViewModel(applicationUser.Id.ToString(), $"{applicationUser.Prenom} {applicationUser.Nom}".Trim(),
            applicationUser.Email!, applicationUser.Nom, applicationUser.Prenom, await userManager.GetRolesAsync(applicationUser)));
    }

    // The refresh cookie is sufficient to identify the family; logout also works after JWT expiry.
    [HttpPost("logout"), AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue(CookieAuth.RefreshCookieName, out var refreshTokenValue))
            await authService.LogoutAsync(refreshTokenValue);
        ClearRefreshCookie();
        return NoContent();
    }

    private void ClearRefreshCookie() =>
        Response.Cookies.Delete(CookieAuth.RefreshCookieName, CookieAuth.CreateCookieOptions(configuration, hostingEnvironment));

    private IActionResult ToHttpResult(ServiceResult<LoginViewModel> authenticationResult)
    {
        if (authenticationResult.IsSuccess)
        {
            var loginResponse = authenticationResult.Data!;
            Response.Cookies.Append(CookieAuth.RefreshCookieName, loginResponse.RefreshToken,
                CookieAuth.CreateCookieOptions(configuration, hostingEnvironment, loginResponse.RefreshTokenExpiresAt));
            return Ok(loginResponse);
        }
        var statusCode = authenticationResult.ErrorType switch
        {
            ServiceErrorType.ValidationError => 400,
            ServiceErrorType.Unauthorized or ServiceErrorType.AccountLockedOut or ServiceErrorType.NotFound => 401,
            ServiceErrorType.Forbidden => 403,
            ServiceErrorType.Conflict => 409,
            _ => 500
        };
        return StatusCode(statusCode, new { message = authenticationResult.Message, errors = authenticationResult.Errors });
    }
}
