using Atelier.Services;
using Atelier.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("revenue")]
    public async Task<ActionResult<MonthlyRevenueViewModel>> GetMonthlyRevenue(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetMonthlyRevenue(cancellationToken));

    [HttpGet("low-stock")]
    public async Task<ActionResult<LowStockViewModel>> GetLowStock(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetLowStock(cancellationToken));

    [HttpGet("recent-orders")]
    public async Task<ActionResult<IReadOnlyList<RecentOrderViewModel>>> GetRecentOrders(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetRecentOrders(cancellationToken));
}
