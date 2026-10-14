using Atelier.Enums;

namespace Atelier.ViewModels
{
    public class LoginViewModel
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; }
        public int UserId { get; set; }
        public UserRole Role { get; set; }

    }
}
