using Atelier.Modeles;

namespace Atelier.Services
{
    public interface IJwtService
    {
        public string GenerateAccessToken(ApplicationUser user);
        public string GenerateRefreshToken();
    }
}
