using Atelier.Common;
using Atelier.Data;
using Atelier.Modeles;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Atelier.Services
{
    public class AuthService : IAuthService
    {
        public readonly AtelierDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        public AuthService(AtelierDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
        }

        public async Task<ServiceResult<LoginViewModel>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<LoginViewModel>.Fail("Identifiants incorrects", ServiceErrorType.NotFound);
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return ServiceResult<LoginViewModel>.Fail("Compte verrouillé suite à trop de tentatives incorrectes. Réessayez dans quelques minutes.", ServiceErrorType.AccountLockedOut);
                }
                return ServiceResult<LoginViewModel>.Fail("Identifiants incorrets", ServiceErrorType.Unauthorized);
            }
            var roles = _userManager.GetRolesAsync(user).Result;
            var accessTokentoken = _jwtService.GenerateAccessToken(user.Id.ToString(), user.Email, user.Prenom, user.Nom, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var loginViewModel = new LoginViewModel
            {
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                Token = accessTokentoken,
                RefreshToken = refreshToken,
                Roles = roles

            };
            return ServiceResult<LoginViewModel>.Ok(loginViewModel, "Authentification réussie.");
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
