namespace Atelier.Modeles;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string SecurityStamp { get; set; } = string.Empty;
    public Guid FamilyId { get; set; }
    public RefreshTokenFamily Family { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public int? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
    public byte[] Version { get; set; } = [];
    public bool IsActive => UsedAt is null && RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
}
