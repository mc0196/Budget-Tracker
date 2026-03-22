using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;
using BudgetTracker.Domain.Exceptions;
using FluentAssertions;

namespace BudgetTracker.Tests.Unit;

/// <summary>
/// Tests for Transaction domain entity business rules.
/// The domain is the most critical layer — these tests have zero infrastructure dependencies.
/// </summary>
public class TransactionEntityTests
{
    private static readonly Guid ValidAccountId = Guid.NewGuid();

    // ── Construction ────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidArguments_CreatesTransaction()
    {
        var transaction = new Transaction(
            accountId: ValidAccountId,
            date: new DateOnly(2026, 1, 15),
            description: "Salary January",
            amount: 2500m,
            type: TransactionType.Income);

        transaction.Id.Should().NotBeEmpty();
        transaction.AccountId.Should().Be(ValidAccountId);
        transaction.Description.Should().Be("Salary January");
        transaction.Amount.Should().Be(2500m);
        transaction.Type.Should().Be(TransactionType.Income);
        transaction.CategoryId.Should().BeNull();
        transaction.IsManuallyCreated.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithZeroAmount_ThrowsDomainException()
    {
        var act = () => new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Test", 0m, TransactionType.Expense);

        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ThrowsDomainException()
    {
        var act = () => new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Test", -10m, TransactionType.Expense);

        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Constructor_WithEmptyDescription_ThrowsArgumentException()
    {
        var act = () => new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "  ", 100m, TransactionType.Expense);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyAccountId_ThrowsArgumentException()
    {
        var act = () => new Transaction(Guid.Empty, new DateOnly(2026, 1, 1), "Test", 100m, TransactionType.Expense);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_TrimsDescriptionWhitespace()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "  Netflix  ", 15m, TransactionType.Expense);

        transaction.Description.Should().Be("Netflix");
    }

    // ── AssignCategory ───────────────────────────────────────────────────────

    [Fact]
    public void AssignCategory_SetsCategoryIdAndUpdatesTimestamp()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Netflix", 15m, TransactionType.Expense);
        var before = transaction.UpdatedAt;
        var categoryId = Guid.NewGuid();

        transaction.AssignCategory(categoryId);

        transaction.CategoryId.Should().Be(categoryId);
        transaction.UpdatedAt.Should().BeOnOrAfter(before);
    }

    // ── UpdateDescription ────────────────────────────────────────────────────

    [Fact]
    public void UpdateDescription_WithValidValue_UpdatesAndBumpsTimestamp()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Old", 100m, TransactionType.Expense);

        transaction.UpdateDescription("New description");

        transaction.Description.Should().Be("New description");
    }

    [Fact]
    public void UpdateDescription_WithBlankString_ThrowsArgumentException()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Old", 100m, TransactionType.Expense);

        var act = () => transaction.UpdateDescription("   ");

        act.Should().Throw<ArgumentException>();
    }

    // ── UpdateAmount ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateAmount_WithPositiveValue_UpdatesAmount()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Test", 100m, TransactionType.Expense);

        transaction.UpdateAmount(200m);

        transaction.Amount.Should().Be(200m);
    }

    [Fact]
    public void UpdateAmount_WithZero_ThrowsDomainException()
    {
        var transaction = new Transaction(ValidAccountId, new DateOnly(2026, 1, 1), "Test", 100m, TransactionType.Expense);

        var act = () => transaction.UpdateAmount(0m);

        act.Should().Throw<DomainException>();
    }
}
