using System.Text.Json.Serialization;

namespace Atelier.ViewModels;

public class LoginViewModel
{
    public string Token { get; set; } = string.Empty;
    // Internal transport to the controller only. Never serialize the refresh secret.
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    [JsonIgnore]
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public required UserViewModel User { get; set; }
}
