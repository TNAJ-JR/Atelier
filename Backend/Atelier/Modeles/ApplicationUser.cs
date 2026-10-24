using Atelier.Enums;
using Microsoft.AspNetCore.Identity;

namespace Atelier.Modeles
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
