using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Application.Services;

/// <summary>
/// Computes all aggregated metrics for the dashboard in a single method call.
/// Deliberately kept simple: fetch → aggregate in memory.
/// For large datasets, move aggregations to SQL via EF Core queries.
/// </summary>
public class DashboardService
{
    private readonly ITransactionRepository _transactionRepository;

    public DashboardService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<DashboardDto> GetDashboardAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        // Summary cards + pie chart: scoped to the selected month
        var monthFrom = new DateOnly(year, month, 1);
        var monthTo   = monthFrom.AddMonths(1).AddDays(-1);

        // Bar chart: full selected year
        var yearFrom = new DateOnly(year, 1, 1);
        var yearTo   = new DateOnly(year, 12, 31);

        var monthTransactions = await _transactionRepository.GetByPeriodAsync(monthFrom, monthTo, cancellationToken);
        var yearTransactions  = await _transactionRepository.GetByPeriodAsync(yearFrom,  yearTo,  cancellationToken);

        var totalIncome   = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var netBalance    = totalIncome - totalExpenses;

        var spendingByCategory = BuildCategorySpending(monthTransactions, totalExpenses);
        var monthlyTrend       = BuildMonthlyTrend(yearTransactions);

        return new DashboardDto(
            TotalIncome: totalIncome,
            TotalExpenses: totalExpenses,
            NetBalance: netBalance,
            SpendingByCategory: spendingByCategory,
            MonthlyTrend: monthlyTrend);
    }

    private static IReadOnlyList<CategorySpendingDto> BuildCategorySpending(
        IReadOnlyList<Domain.Entities.Transaction> transactions,
        decimal totalExpenses)
    {
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

        var grouped = expenseTransactions
            .GroupBy(t => new
            {
                CategoryName = t.Category?.Name ?? "Uncategorized",
                CategoryColor = t.Category?.Color
            })
            .Select(g => new
            {
                g.Key.CategoryName,
                g.Key.CategoryColor,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return grouped.Select(g => new CategorySpendingDto(
            CategoryName: g.CategoryName,
            CategoryColor: g.CategoryColor,
            Amount: g.Amount,
            Percentage: totalExpenses > 0 ? Math.Round(g.Amount / totalExpenses * 100, 1) : 0
        )).ToList();
    }

    private static IReadOnlyList<MonthlyTrendDto> BuildMonthlyTrend(
        IReadOnlyList<Domain.Entities.Transaction> transactions)
    {
        return transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var expenses = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                return new MonthlyTrendDto(
                    Year: g.Key.Year,
                    Month: g.Key.Month,
                    MonthLabel: new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Income: income,
                    Expenses: expenses,
                    Net: income - expenses);
            })
            .ToList();
    }
}
