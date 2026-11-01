using System.ComponentModel.DataAnnotations;
using Atelier.Common;
using Atelier.Data;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Services;

public sealed class ProductService(AtelierDbContext context, TimeProvider clock) : IProductService
{
    public async Task<ServiceResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await context.Products.AsNoTracking().ToListAsync();
        return ServiceResult<IEnumerable<Product>>.Ok(products);
    }

    public async Task<ServiceResult<ProductViewModel>> CreateProduct(
        CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, true))
            return ServiceResult<ProductViewModel>.Fail("Les données du produit sont invalides.",
                ServiceErrorType.ValidationError, validationResults.Select(result => result.ErrorMessage!).ToList());

        var sku = request.Sku.Trim().ToUpperInvariant();
        if (await context.Products.AnyAsync(product => product.Sku.ToUpper() == sku, cancellationToken))
            return DuplicateSku();

        var product = new Product
        {
            Sku = sku,
            Name = request.Name.Trim(),
            PriceCents = request.PriceCents,
            Stock = request.Stock,
            Category = request.Category,
            Active = request.Active,
            CreatedAt = clock.GetUtcNow()
        };
        context.Products.Add(product);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // The unique index also protects concurrent requests that passed the initial check.
            context.Entry(product).State = EntityState.Detached;
            return DuplicateSku();
        }

        return ServiceResult<ProductViewModel>.Ok(new ProductViewModel(
            product.Id, product.Sku, product.Name, product.PriceCents, product.Stock,
            product.Category.ToString().ToLowerInvariant(), product.Active, product.CreatedAt));
    }

    private static ServiceResult<ProductViewModel> DuplicateSku()
        => ServiceResult<ProductViewModel>.Fail("Ce SKU est déjà utilisé par un autre produit.", ServiceErrorType.Conflict);
}
