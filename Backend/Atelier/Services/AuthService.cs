using Microsoft.EntityFrameworkCore;

namespace Atelier.Services
{
    public class AuthService
    {
        public readonly DbContext _context;

        public AuthService(DbContext context)
        {
            _context = context;
        }


    }
}
