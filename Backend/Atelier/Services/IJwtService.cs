namespace Atelier.Services
{
    public interface IJwtService
    {
        public string GenerateAccessToken(string userId, string email, string prenom, string nom,
            IList<string> roles);
        public string GenerateRefreshToken();
    }
}
