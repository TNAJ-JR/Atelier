namespace Atelier.ViewModels;

public class LoginViewModel
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public required UserViewModel User { get; set; }
}

public record UserViewModel(string Id, string Name, string Email, string Nom, string Prenom, IList<string> Roles);
