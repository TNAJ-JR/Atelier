namespace Atelier.ViewModels;

public sealed record MonthlyRevenueViewModel(
    int Year,
    int Month,
    string TimeZone,
    long RevenueCents,
    int OrderCount);

public sealed record LowStockProductViewModel(int Id, string Name, string Sku, int Stock, bool Active);

public sealed record LowStockViewModel(int Threshold, IReadOnlyList<LowStockProductViewModel> Products);

public sealed record RecentOrderViewModel(
    int Id,
    int CustomerId,
    string CustomerName,
    DateTimeOffset CreatedAt,
    string Status,
    long TotalCents,
    int LineCount);
