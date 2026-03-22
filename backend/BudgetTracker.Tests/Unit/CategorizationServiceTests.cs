using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BudgetTracker.Tests.Unit;

public class CategorizationServiceTests
{
    private static readonly Guid AccountId = new("00000000-0000-0000-0000-000000000001");
    private static readonly Guid EntertainmentId = new("10000000-0000-0000-0000-000000000006");
    private static readonly Guid SalaryId = new("10000000-0000-0000-0000-000000000001");

    private static Transaction MakeTransaction(string description)
        => new(AccountId, new DateOnly(2026, 1, 1), description, 10m, TransactionType.Expense);

    [Fact]
    public async Task ApplyRules_MatchingKeyword_AssignsCategory()
    {
        var rule = new CategorizationRule("netflix", EntertainmentId, priority: 5);
        var ruleRepo = new Mock<ICategorizationRuleRepository>();
        ruleRepo.Setup(r => r.GetActiveRulesAsync(default)).ReturnsAsync([rule]);

        var service = new CategorizationService(ruleRepo.Object, NullLogger<CategorizationService>.Instance);
        var transactions = new List<Transaction> { MakeTransaction("Netflix monthly charge") };

        await service.ApplyRulesAsync(transactions);

        transactions[0].CategoryId.Should().Be(EntertainmentId);
    }

    [Fact]
    public async Task ApplyRules_NoMatchingKeyword_LeavesUncategorized()
    {
        var rule = new CategorizationRule("netflix", EntertainmentId);
        var ruleRepo = new Mock<ICategorizationRuleRepository>();
        ruleRepo.Setup(r => r.GetActiveRulesAsync(default)).ReturnsAsync([rule]);

        var service = new CategorizationService(ruleRepo.Object, NullLogger<CategorizationService>.Instance);
        var transactions = new List<Transaction> { MakeTransaction("Spotify premium") };

        await service.ApplyRulesAsync(transactions);

        transactions[0].CategoryId.Should().BeNull();
    }

    [Fact]
    public async Task ApplyRules_HigherPriorityRuleWins()
    {
        // Both rules match "salary payroll", but salary rule has higher priority
        var lowPriority  = new CategorizationRule("payroll", EntertainmentId, priority: 1);
        var highPriority = new CategorizationRule("salary",  SalaryId,        priority: 10);

        var ruleRepo = new Mock<ICategorizationRuleRepository>();
        // Rules returned ordered by priority descending (as the repo contract states)
        ruleRepo.Setup(r => r.GetActiveRulesAsync(default)).ReturnsAsync([highPriority, lowPriority]);

        var service = new CategorizationService(ruleRepo.Object, NullLogger<CategorizationService>.Instance);
        var transactions = new List<Transaction> { MakeTransaction("salary payroll january") };

        await service.ApplyRulesAsync(transactions);

        transactions[0].CategoryId.Should().Be(SalaryId);
    }

    [Fact]
    public async Task ApplyRules_NoRules_DoesNotThrow()
    {
        var ruleRepo = new Mock<ICategorizationRuleRepository>();
        ruleRepo.Setup(r => r.GetActiveRulesAsync(default)).ReturnsAsync([]);

        var service = new CategorizationService(ruleRepo.Object, NullLogger<CategorizationService>.Instance);
        var transactions = new List<Transaction> { MakeTransaction("Netflix") };

        var act = () => service.ApplyRulesAsync(transactions);

        await act.Should().NotThrowAsync();
        transactions[0].CategoryId.Should().BeNull();
    }
}
