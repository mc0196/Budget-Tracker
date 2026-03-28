using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns dashboard metrics for the given year+month.
    /// Summary cards and pie chart are scoped to that month;
    /// the monthly trend covers the full year.
    /// Defaults to the current month if no params are provided.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        var dashboard = await _dashboardService.GetDashboardAsync(y, m, cancellationToken);
        return Ok(dashboard);
    }
}
