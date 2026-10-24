namespace Atelier.Modeles;

// One login creates one family. Revoking it invalidates every token in that session.
public class RefreshTokenFamily
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset? RevokedAt { get; set; }
}
