using Atelier.Common;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.ViewModels;

namespace Atelier.Services;

public interface IProductService
{
    Task<ServiceResult<IEnumerable<Product>>> GetProducts();
    Task<ServiceResult<ProductViewModel>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken = default);
}
