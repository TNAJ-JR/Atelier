using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Atelier.Controllers;
using Atelier.Data;
using Atelier.Enums;
using Atelier.Modeles;
using Atelier.Requests;
using Atelier.Services;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Atelier.Products.Tests;

public sealed class ProductCreationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private WebApplication application = null!;
    private HttpClient client = null!;
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-29T12:00:00Z");

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<AtelierDbContext>(options => options.UseSqlite(connection));
        builder.Services.AddSingleton<TimeProvider>(new FrozenClock());
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddControllers().AddApplicationPart(typeof(ProductController).Assembly);
        application = builder.Build();
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapControllers();
        await application.StartAsync();
        using var scope = application.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AtelierDbContext>().Database.EnsureCreatedAsync();
        client = application.GetTestClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", "Staff");
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await application.DisposeAsync();
        await connection.DisposeAsync();
    }

    private static CreateProductRequest Request() => new()
    {
        Sku = "  boi-001  ", Name = "  Planche  ", PriceCents = 2500,
        Stock = 3, Category = ProductCategory.Bois
    };

    [Theory]
    [InlineData("Staff")]
    [InlineData("Admin")]
    public async Task CreatesProductAndReturns201WithServerManagedFields(string role)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        using var response = await client.PostAsJsonAsync("/api/products", Request());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        Assert.NotNull(product);
        Assert.True(product.Id > 0);
        Assert.Equal("BOI-001", product.Sku);
        Assert.Equal("Planche", product.Name);
        Assert.Equal(2500, product.PriceCents);
        Assert.Equal(3, product.Stock);
        Assert.Equal("bois", product.Category);
        Assert.True(product.Active);
        Assert.Equal(Now, product.CreatedAt);
        using var scope = application.Services.CreateScope();
        var stored = await scope.ServiceProvider.GetRequiredService<AtelierDbContext>().Products.SingleAsync();
        Assert.Equal(product.Id, stored.Id);
    }

    [Fact]
    public async Task DuplicateSkuReturns422AndFieldErrorWithoutInserting()
    {
        using var scope = application.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AtelierDbContext>();
        context.Products.Add(new Product { Sku = "boi-001", Name = "Existant", CreatedAt = Now });
        await context.SaveChangesAsync();
        using var response = await client.PostAsJsonAsync("/api/products", Request());
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("sku", out _));
        Assert.Equal(1, await context.Products.CountAsync());
    }

    [Theory]
    [InlineData("sku")]
    [InlineData("name")]
    [InlineData("price")]
    [InlineData("stock")]
    [InlineData("category")]
    [InlineData("long-sku")]
    [InlineData("long-name")]
    public async Task InvalidFieldsReturn400AndDoNotInsert(string field)
    {
        var request = Request();
        switch (field)
        {
            case "sku": request.Sku = "  "; break;
            case "name": request.Name = "  "; break;
            case "price": request.PriceCents = -1; break;
            case "stock": request.Stock = -1; break;
            case "category": request.Category = (ProductCategory)999; break;
            case "long-sku": request.Sku = new string('A', 451); break;
            case "long-name": request.Name = new string('A', 201); break;
        }
        using var response = await client.PostAsJsonAsync("/api/products", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = application.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AtelierDbContext>().Products.CountAsync());
    }

    [Fact]
    public async Task MissingRequiredNumericFieldsAreNotDefaultedSilently()
    {
        using var response = await client.PostAsJsonAsync("/api/products", new { sku = "TEST", name = "Produit" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Client", HttpStatusCode.Forbidden)]
    public async Task AnonymousAndCustomerCannotCreate(string? role, HttpStatusCode expected)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        if (role is not null) client.DefaultRequestHeaders.Add("X-Test-Role", role);
        using var response = await client.PostAsJsonAsync("/api/products", Request());
        Assert.Equal(expected, response.StatusCode);
    }

    [Fact]
    public async Task ServerIgnoresClientSuppliedIdentityDateAndOrderItems()
    {
        using var response = await client.PostAsJsonAsync("/api/products", new
        {
            id = 999, sku = "TEST", name = "Produit", priceCents = 0, stock = 0,
            category = 0, active = false, createdAt = "2000-01-01T00:00:00Z",
            orderItems = new[] { new { orderId = 1, quantity = 100, unitPriceCents = 0 } }
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        Assert.NotNull(result);
        Assert.NotEqual(999, result.Id);
        Assert.Equal(Now, result.CreatedAt);
        Assert.False(result.Active);
        using var scope = application.Services.CreateScope();
        Assert.Equal(0, await scope.ServiceProvider.GetRequiredService<AtelierDbContext>().OrderItems.CountAsync());
    }

    [Fact]
    public async Task ServiceAlsoRejectsInvalidDataAndPropagatesCancellation()
    {
        using var scope = application.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IProductService>();
        var invalid = Request();
        invalid.Stock = -1;
        Assert.False((await service.CreateProduct(invalid)).IsSuccess);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.CreateProduct(Request(), cancellation.Token));
    }

    private sealed class FrozenClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-Role", out var role))
            return Task.FromResult(AuthenticateResult.NoResult());
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, role.ToString())], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
