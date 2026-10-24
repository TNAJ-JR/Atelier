using System.ComponentModel.DataAnnotations;

namespace Atelier.Requests
{
    public record RefreshRequest([Required, MaxLength(256)] string RefreshToken);
}
