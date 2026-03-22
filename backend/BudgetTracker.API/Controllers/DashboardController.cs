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
    /// Returns all dashboard metrics for a given date range.
    /// Defaults to the last 12 months if no range is provided.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _dashboardService.GetDashboardAsync(from, to, cancellationToken);
        return Ok(dashboard);
    }
}
