using Atelier.ViewModels;

namespace Atelier.Services;

public interface IDashboardService
{
    Task<MonthlyRevenueViewModel> GetMonthlyRevenue(CancellationToken cancellationToken = default);
    Task<LowStockViewModel> GetLowStock(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RecentOrderViewModel>> GetRecentOrders(CancellationToken cancellationToken = default);
}
