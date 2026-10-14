using Atelier.Data;

namespace Atelier.Services
{
    public class AuthService
    {
        public readonly AtelierDbContext _context;
        public AuthService(AtelierDbContext context)
        {
            _context = context;
        }

        //public async Task<ServiceResult<ApplicationUser>> AuthenticateAsync(string email, string password)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        //    if (user == null)
        //    {
        //        return ServiceResult<ApplicationUser>.Fail("Utilisateur non trouvé.", ServiceErrorType.NotFound);
        //    }
        //    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        //    if (!isPasswordValid)
        //    {
        //        return ServiceResult<ApplicationUser>.Fail("Mot de passe incorrect.", ServiceErrorType.Unauthorized);
        //    }
        //    return ServiceResult<ApplicationUser>.Ok(user, "Authentification réussie.");
        //}

        //public async Task<string> HashPasswordAsync(string Password, ApplicationUser applicationUser)
        //{
        //    if (string.IsNullOrWhiteSpace(Password))
        //    {
        //        throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(Password));
        //    }
        //    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);
        //    applicationUser.PasswordHash = hashedPassword;
        //    _context.Users.Update(applicationUser);
        //    await _context.SaveChangesAsync();
        //    return hashedPassword;
        //}
    }
}
