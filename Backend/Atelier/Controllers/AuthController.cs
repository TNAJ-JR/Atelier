using Atelier.Common;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.Services;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Atelier.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthController(IAuthService authService, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request) =>
        ToHttpResult(await authService.LoginAsync(request.Email, request.Password));

    [HttpPost("refresh"), AllowAnonymous, EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request) =>
        ToHttpResult(await authService.RefreshAsync(request.RefreshToken));

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await userManager.FindByIdAsync(User.FindFirst("sub")!.Value);
        if (user is null) return Unauthorized(new { message = "Session invalide." });
        return Ok(new UserViewModel(user.Id.ToString(), $"{user.Prenom} {user.Nom}".Trim(),
            user.Email!, user.Nom, user.Prenom, await userManager.GetRolesAsync(user)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync(int.Parse(User.FindFirst("sub")!.Value),
            int.Parse(User.FindFirst("sid")!.Value));
        return NoContent();
    }

    private IActionResult ToHttpResult(ServiceResult<LoginViewModel> result)
    {
        if (result.IsSuccess) return Ok(result.Data);
        var status = result.ErrorType switch
        {
            ServiceErrorType.ValidationError => 400,
            ServiceErrorType.Unauthorized or ServiceErrorType.AccountLockedOut or ServiceErrorType.NotFound => 401,
            ServiceErrorType.Forbidden => 403,
            ServiceErrorType.Conflict => 409,
            _ => 500
        };
        return StatusCode(status, new { message = result.Message, errors = result.Errors });
    }
}
