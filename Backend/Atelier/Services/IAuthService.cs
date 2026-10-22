using Atelier.Common;
using Atelier.ViewModels;

namespace Atelier.Services
{
    public interface IAuthService
    {
        public Task<ServiceResult<LoginViewModel>> LoginAsync(string email, string password);

    }
}
