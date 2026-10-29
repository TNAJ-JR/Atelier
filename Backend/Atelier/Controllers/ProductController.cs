using Atelier.Modeles;
using Atelier.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController(IProductService productService) : ControllerBase
    {

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await productService.GetProducts();
            return Ok(products);
        }

        public async Task<IActionResult> Create()
        {
            var result = await productService.CreateProduct();
        }
    }
}
