using System.ComponentModel.DataAnnotations;

namespace Atelier.Requests;

public record LoginRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MaxLength(256)] string Password);


