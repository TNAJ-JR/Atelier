using Atelier.Common;
using Atelier.ViewModels;

namespace Atelier.Services;

public interface IAuthService
{
    Task<ServiceResult<LoginViewModel>> LoginAsync(string email, string password);
    Task<ServiceResult<LoginViewModel>> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId, int sessionId);
}
