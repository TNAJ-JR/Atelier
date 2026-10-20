namespace Atelier.Modeles
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset RevokedAt { get; set; }

        public bool IsActive =>
            RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    }
}
