using Atelier.Controllers;
using Atelier.Data;
using Atelier.Enums;
using Atelier.Modeles;
using Atelier.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Atelier.Dashboard.Tests;

public sealed class DashboardServiceTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private AtelierDbContext context = null!;
    private static readonly TimeZoneInfo Toronto = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        context = new AtelierDbContext(new DbContextOptionsBuilder<AtelierDbContext>().UseSqlite(connection).Options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await context.DisposeAsync();
        await connection.DisposeAsync();
    }

    private DashboardService Service(string now = "2026-03-15T12:00:00Z")
        => new(context, new FrozenClock(DateTimeOffset.Parse(now)), Toronto);

    [Fact]
    public async Task EmptyDatabaseReturnsZeroAndEmptyLists()
    {
        var revenue = await Service().GetMonthlyRevenue();
        Assert.Equal(0L, revenue.RevenueCents);
        Assert.Equal(0, revenue.OrderCount);
        Assert.Empty((await Service().GetLowStock()).Products);
        Assert.Empty(await Service().GetRecentOrders());
    }

    [Fact]
    public async Task RevenueUsesTorontoMonthAcrossDaylightSavingAndOnlyEligibleStatuses()
    {
        await SeedBasics();
        // March begins at 05:00 UTC and ends at 04:00 UTC because the offset changes.
        var start = DateTimeOffset.Parse("2026-03-01T05:00:00Z");
        var end = DateTimeOffset.Parse("2026-04-01T04:00:00Z");
        var first = Order(1, start, OrderStatus.Confirmed, 2, 125);
        first.Items.Add(new OrderItem { ProductId = 2, Quantity = 1, UnitPriceCents = 50 });
        context.Orders.AddRange(
            first,
            Order(2, end.AddMilliseconds(-1), OrderStatus.Shipped, 1, 500),
            Order(3, start.AddMilliseconds(-1), OrderStatus.Confirmed, 1, 9000),
            Order(4, end, OrderStatus.Confirmed, 1, 9000),
            Order(5, start, OrderStatus.Draft, 1, 9000),
            Order(6, start, OrderStatus.Cancelled, 1, 9000),
            new Order { Id = 7, CustomerId = 1, Status = OrderStatus.Confirmed, CreatedAt = start });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await Service().GetMonthlyRevenue();
        Assert.Equal(800L, result.RevenueCents);
        Assert.Equal(3, result.OrderCount);
        Assert.Equal(2026, result.Year);
        Assert.Equal(3, result.Month);
        Assert.Equal("America/Toronto", result.TimeZone);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task CurrentMonthUsesTorontoDateRatherThanUtcDate()
    {
        var result = await Service("2026-04-01T00:30:00Z").GetMonthlyRevenue();
        Assert.Equal(3, result.Month);
        var january = await Service("2027-01-15T12:00:00Z").GetMonthlyRevenue();
        Assert.Equal(2027, january.Year);
        Assert.Equal(1, january.Month);
    }

    [Fact]
    public async Task TotalsUseHistoricalPricesAndDoNotOverflowInt32()
    {
        await SeedBasics();
        context.Orders.Add(Order(1, DateTimeOffset.Parse("2026-03-10T12:00:00Z"), OrderStatus.Confirmed, 2, 1_500_000_000));
        await context.SaveChangesAsync();

        Assert.Equal(3_000_000_000L, (await Service().GetMonthlyRevenue()).RevenueCents);
        var recent = Assert.Single(await Service().GetRecentOrders());
        Assert.Equal(3_000_000_000L, recent.TotalCents);
        Assert.Equal(1, recent.LineCount);
        Assert.Equal("confirmed", recent.Status);
        Assert.Equal("Client de test", recent.CustomerName);
    }

    [Fact]
    public async Task LowStockUsesExclusiveThresholdIncludesInactiveAndSortsDeterministically()
    {
        context.Products.AddRange(
            Product(1, "Zéro", 0, false),
            Product(2, "B", 4),
            Product(3, "A", 4),
            Product(4, "Seuil", 5),
            Product(5, "Au-dessus", 6));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await Service().GetLowStock();
        Assert.Equal(5, result.Threshold);
        Assert.Equal(new[] { 1, 3, 2 }, result.Products.Select(product => product.Id));
        Assert.False(result.Products[0].Active);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task RecentOrdersReturnFiveAcrossAllStatusesWithStableTieBreaker()
    {
        await SeedBasics();
        var date = DateTimeOffset.Parse("2026-03-10T12:00:00Z");
        for (var id = 1; id <= 6; id++)
            context.Orders.Add(Order(id, date, (OrderStatus)(id % 4), 1, id * 100));
        context.Orders.Add(new Order { Id = 7, CustomerId = 1, CreatedAt = date.AddMinutes(1) });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await Service().GetRecentOrders();
        Assert.Equal(new[] { 7, 6, 5, 4, 3 }, results.Select(order => order.Id));
        Assert.Equal(new[] { "draft", "shipped", "confirmed", "draft", "cancelled" }, results.Select(order => order.Status));
        Assert.Equal(0L, results[0].TotalCents);
        Assert.Equal(0, results[0].LineCount);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task RequestCancellationIsPropagated()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().GetMonthlyRevenue(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().GetLowStock(cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service().GetRecentOrders(cancellation.Token));
    }

    [Fact]
    public void DashboardRequiresAuthentication()
    {
        Assert.Contains(typeof(DashboardController).GetCustomAttributes(true), attribute => attribute is AuthorizeAttribute);
        Assert.DoesNotContain(typeof(DashboardController).GetCustomAttributes(true), attribute => attribute is AllowAnonymousAttribute);
    }

    private async Task SeedBasics()
    {
        context.Customers.Add(new Customer { Id = 1, Name = "Client de test", Email = "client@example.test" });
        context.Products.AddRange(Product(1, "Produit A", 10), Product(2, "Produit B", 10));
        await context.SaveChangesAsync();
    }

    private static Product Product(int id, string name, int stock, bool active = true)
        => new() { Id = id, Sku = $"TEST-{id}", Name = name, Stock = stock, Active = active, PriceCents = 999 };

    private static Order Order(int id, DateTimeOffset createdAt, OrderStatus status, int quantity, int price)
        => new()
        {
            Id = id, CustomerId = 1, CreatedAt = createdAt, Status = status,
            Items = [new OrderItem { ProductId = 1, Quantity = quantity, UnitPriceCents = price }]
        };

    private sealed class FrozenClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
