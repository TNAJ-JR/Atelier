using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atelier.Security;

public class CookieCsrfFilter(IAntiforgery antiforgeryService, IConfiguration configuration,
    IWebHostEnvironment hostingEnvironment) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext filterContext)
    {
        var httpRequest = filterContext.HttpContext.Request;
        if (!CookieAuth.IsCookieEndpoint(httpRequest.Path)) return;
        if (!httpRequest.IsHttps)
        {
            filterContext.Result = new BadRequestObjectResult(new { message = "HTTPS est requis." });
            return;
        }
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? (hostingEnvironment.IsDevelopment() ? ["http://localhost:5173"] : []);
        // CORS alone does not stop a forged form POST from executing.
        if (httpRequest.Headers.TryGetValue("Origin", out var requestOrigin) &&
            !allowedOrigins.Contains(requestOrigin.ToString(), StringComparer.OrdinalIgnoreCase) &&
            !string.Equals(requestOrigin.ToString(), $"{httpRequest.Scheme}://{httpRequest.Host}", StringComparison.OrdinalIgnoreCase))
        {
            filterContext.Result = new ObjectResult(new { message = "Origine non autorisée." }) { StatusCode = 403 };
            return;
        }
        if (HttpMethods.IsGet(httpRequest.Method) || HttpMethods.IsHead(httpRequest.Method) ||
            HttpMethods.IsOptions(httpRequest.Method)) return;
        try { await antiforgeryService.ValidateRequestAsync(filterContext.HttpContext); }
        catch (AntiforgeryValidationException)
        {
            filterContext.Result = new BadRequestObjectResult(new
            {
                code = "csrf_invalid", message = "Protection CSRF invalide. Rechargez la demande."
            });
        }
    }
}
