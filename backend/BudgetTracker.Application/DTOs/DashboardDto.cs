namespace BudgetTracker.Application.DTOs;

/// <summary>
/// All data needed to render the dashboard in a single API call.
/// One round-trip = one page load. Simple and efficient for an MVP.
/// </summary>
public record DashboardDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetBalance,
    IReadOnlyList<CategorySpendingDto> SpendingByCategory,
    IReadOnlyList<MonthlyTrendDto> MonthlyTrend
);

public record CategorySpendingDto(
    string CategoryName,
    string? CategoryColor,
    decimal Amount,
    decimal Percentage
);

public record MonthlyTrendDto(
    int Year,
    int Month,
    string MonthLabel,
    decimal Income,
    decimal Expenses,
    decimal Net
);
