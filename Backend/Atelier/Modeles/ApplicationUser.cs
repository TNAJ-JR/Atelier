using Atelier.Enums;
using Microsoft.AspNetCore.Identity;

namespace Atelier.Modeles
{
    public class ApplicationUser : IdentityUser<int>
    {
        public required string Email { get; set; }

        /// <summary>Hachage BCrypt. Ne sort jamais de l'API — voir UserDto.</summary>
        public required string PasswordHash { get; set; }

        public UserRole Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
