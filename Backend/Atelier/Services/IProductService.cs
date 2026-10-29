using Atelier.Common;
using Atelier.Modeles;

namespace Atelier.Services
{
    public interface IProductService
    {
        Task<ServiceResult<IEnumerable<Product>>> GetProducts();
    }
}
