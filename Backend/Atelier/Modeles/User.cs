using Atelier.Enums;

namespace Atelier.Modeles
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }

        /// <summary>Hachage BCrypt. Ne sort jamais de l'API — voir UserDto.</summary>
        public required string PasswordHash { get; set; }

        public UserRole Role { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
