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
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveFrom = from ?? new DateOnly(DateTimeOffset.UtcNow.Year - 1, 1, 1);
        var effectiveTo = to ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);

        var transactions = await _transactionRepository.GetByPeriodAsync(
            effectiveFrom, effectiveTo, cancellationToken);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        var netBalance = totalIncome - totalExpenses;

        var spendingByCategory = BuildCategorySpending(transactions, totalExpenses);
        var monthlyTrend = BuildMonthlyTrend(transactions);

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
