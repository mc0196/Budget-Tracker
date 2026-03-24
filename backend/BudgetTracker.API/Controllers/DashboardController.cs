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
    /// Returns dashboard metrics for the given month/year.
    /// Cards and pie chart are filtered by month; bar chart always covers the full year.
    /// Defaults to the current month/year if not provided.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var effectiveYear  = year  ?? now.Year;
        var effectiveMonth = month ?? now.Month;

        var dashboard = await _dashboardService.GetDashboardAsync(
            effectiveYear, effectiveMonth, cancellationToken);
        return Ok(dashboard);
    }
}
