using BudgetTracker.Domain.Enums;
using BudgetTracker.Domain.Exceptions;

namespace BudgetTracker.Domain.Entities;

/// <summary>
/// Core domain entity. Represents a single money movement (debit or credit).
///
/// Design notes:
/// - Amount is always positive; Type (Income/Expense) encodes direction.
///   This avoids the confusion of signed amounts when displaying data.
/// - OriginalText stores the raw bank CSV line for debugging/auditing.
/// - SourceFile lets you trace which import created each transaction.
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? OriginalText { get; private set; }
    public string? SourceFile { get; private set; }
    public bool IsManuallyCreated { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Account Account { get; private set; } = null!;
    public Category? Category { get; private set; }

    private Transaction() { Description = null!; } // EF Core constructor

    public Transaction(
        Guid accountId,
        DateOnly date,
        string description,
        decimal amount,
        TransactionType type,
        string? originalText = null,
        string? sourceFile = null,
        bool isManuallyCreated = false)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("AccountId cannot be empty.", nameof(accountId));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (amount <= 0)
            throw new DomainException("Transaction amount must be greater than zero.");

        Id = Guid.NewGuid();
        AccountId = accountId;
        Date = date;
        Description = description.Trim();
        Amount = amount;
        Type = type;
        OriginalText = originalText;
        SourceFile = sourceFile;
        IsManuallyCreated = isManuallyCreated;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignCategory(Guid categoryId)
    {
        CategoryId = categoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        Description = description.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Transaction amount must be greater than zero.");

        Amount = amount;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
