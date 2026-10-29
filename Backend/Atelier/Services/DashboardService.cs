using Atelier.Data;
using Atelier.Enums;
using Atelier.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Atelier.Services;

public sealed class DashboardService(
    AtelierDbContext context,
    TimeProvider clock,
    TimeZoneInfo timeZone) : IDashboardService
{
    public async Task<MonthlyRevenueViewModel> GetMonthlyRevenue(CancellationToken cancellationToken = default)
    {
        var localNow = TimeZoneInfo.ConvertTime(clock.GetUtcNow(), timeZone);
        var monthStart = new DateTime(localNow.Year, localNow.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(monthStart, timeZone));
        var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(monthStart.AddMonths(1), timeZone));

        // The end is exclusive. Only the order's creation date and current status are stored.
        var orders = context.Orders.AsNoTracking().Where(order =>
            order.CreatedAt >= startUtc && order.CreatedAt < endUtc &&
            (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Shipped));

        // A left join also counts eligible orders with no lines. Amounts use historical prices.
        var lines = orders.SelectMany(
            order => order.Items.DefaultIfEmpty(),
            (order, item) => new
            {
                OrderId = order.Id,
                AmountCents = item == null ? 0L : (long)item.Quantity * item.UnitPriceCents
            });

        var totals = await lines.GroupBy(_ => 1)
            .Select(group => new
            {
                RevenueCents = group.Sum(line => line.AmountCents),
                OrderCount = group.Select(line => line.OrderId).Distinct().Count()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return new MonthlyRevenueViewModel(localNow.Year, localNow.Month, timeZone.Id,
            totals?.RevenueCents ?? 0L, totals?.OrderCount ?? 0);
    }

    public async Task<LowStockViewModel> GetLowStock(CancellationToken cancellationToken = default)
    {
        const int threshold = 5;
        // Include inactive products: a stock of zero remains relevant to the dashboard.
        var products = await context.Products.AsNoTracking()
            .Where(product => product.Stock < threshold)
            .OrderBy(product => product.Stock).ThenBy(product => product.Name).ThenBy(product => product.Id)
            .Select(product => new LowStockProductViewModel(
                product.Id, product.Name, product.Sku, product.Stock, product.Active))
            .ToListAsync(cancellationToken);

        return new LowStockViewModel(threshold, products);
    }

    public async Task<IReadOnlyList<RecentOrderViewModel>> GetRecentOrders(CancellationToken cancellationToken = default)
    {
        var orders = await context.Orders.AsNoTracking()
            .OrderByDescending(order => order.CreatedAt).ThenByDescending(order => order.Id)
            .Take(5)
            .Select(order => new
            {
                order.Id,
                order.CustomerId,
                CustomerName = order.Customer.Name,
                order.CreatedAt,
                order.Status,
                TotalCents = order.Items.Sum(item => (long?)item.Quantity * item.UnitPriceCents) ?? 0L,
                LineCount = order.Items.Count
            })
            .ToListAsync(cancellationToken);

        return orders.Select(order => new RecentOrderViewModel(
            order.Id, order.CustomerId, order.CustomerName, order.CreatedAt,
            order.Status.ToString().ToLowerInvariant(), order.TotalCents, order.LineCount)).ToList();
    }
}
