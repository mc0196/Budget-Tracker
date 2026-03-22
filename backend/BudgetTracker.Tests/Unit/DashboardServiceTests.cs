using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;
using FluentAssertions;
using Moq;

namespace BudgetTracker.Tests.Unit;

public class DashboardServiceTests
{
    private static readonly Guid AccountId = new("00000000-0000-0000-0000-000000000001");

    private static Transaction Income(decimal amount, int month = 1)
        => new(AccountId, new DateOnly(2026, month, 1), "Salary", amount, TransactionType.Income);

    private static Transaction Expense(decimal amount, int month = 1)
        => new(AccountId, new DateOnly(2026, month, 15), "Shopping", amount, TransactionType.Expense);

    [Fact]
    public async Task GetDashboard_CalculatesTotalsCorrectly()
    {
        var transactions = new List<Transaction>
        {
            Income(3000m),
            Expense(500m),
            Expense(200m),
        };

        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.GetByPeriodAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync(transactions);

        var service = new DashboardService(repo.Object);
        var dashboard = await service.GetDashboardAsync();

        dashboard.TotalIncome.Should().Be(3000m);
        dashboard.TotalExpenses.Should().Be(700m);
        dashboard.NetBalance.Should().Be(2300m);
    }

    [Fact]
    public async Task GetDashboard_NoTransactions_ReturnsZeros()
    {
        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.GetByPeriodAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync([]);

        var service = new DashboardService(repo.Object);
        var dashboard = await service.GetDashboardAsync();

        dashboard.TotalIncome.Should().Be(0);
        dashboard.TotalExpenses.Should().Be(0);
        dashboard.NetBalance.Should().Be(0);
        dashboard.SpendingByCategory.Should().BeEmpty();
        dashboard.MonthlyTrend.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboard_MonthlyTrend_GroupsByMonth()
    {
        var transactions = new List<Transaction>
        {
            Income(3000m, month: 1),
            Expense(500m,  month: 1),
            Income(3000m, month: 2),
            Expense(800m,  month: 2),
        };

        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.GetByPeriodAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync(transactions);

        var service = new DashboardService(repo.Object);
        var dashboard = await service.GetDashboardAsync();

        dashboard.MonthlyTrend.Should().HaveCount(2);
        dashboard.MonthlyTrend[0].Income.Should().Be(3000m);
        dashboard.MonthlyTrend[0].Expenses.Should().Be(500m);
        dashboard.MonthlyTrend[1].Expenses.Should().Be(800m);
    }

    [Fact]
    public async Task GetDashboard_SpendingByCategory_CalculatesPercentages()
    {
        var transactions = new List<Transaction>
        {
            Expense(300m), // 75% of total expenses
            Expense(100m), // 25% of total expenses
        };

        var repo = new Mock<ITransactionRepository>();
        repo.Setup(r => r.GetByPeriodAsync(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), default))
            .ReturnsAsync(transactions);

        var service = new DashboardService(repo.Object);
        var dashboard = await service.GetDashboardAsync();

        // Both are "Uncategorized" so they merge into one bucket
        dashboard.SpendingByCategory.Should().HaveCount(1);
        dashboard.SpendingByCategory[0].Amount.Should().Be(400m);
        dashboard.SpendingByCategory[0].Percentage.Should().Be(100m);
    }
}
