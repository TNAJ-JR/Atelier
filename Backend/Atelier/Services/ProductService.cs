using Atelier.Common;
using Atelier.Data;
using Atelier.Modeles;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Services
{

    public class ProductService(AtelierDbContext _context) : IProductService
    {

        public async Task<ServiceResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products.ToListAsync();


            return ServiceResult<IEnumerable<Product>>.Ok(products);
        }

        public async Task<ServiceResult<Product>> CreateProduct()
        {

        }
    }
}
