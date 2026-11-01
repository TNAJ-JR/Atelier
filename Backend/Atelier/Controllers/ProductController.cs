using Atelier.Common;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.Services;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier.Controllers;

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

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<ProductViewModel>> Create(
        [FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.CreateProduct(request, cancellationToken);
        if (result.IsSuccess)
            return StatusCode(StatusCodes.Status201Created, result.Data);

        if (result.ErrorType == ServiceErrorType.Conflict)
        {
            var problem = new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["sku"] = [result.Message]
            })
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = result.Message
            };
            return UnprocessableEntity(problem);
        }

        return BadRequest(new { message = result.Message, errors = result.Errors });
    }
}
