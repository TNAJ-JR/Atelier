namespace Atelier.ViewModels
{
    public class LoginViewModel
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public IList<string> Roles { get; set; }

    }
}
