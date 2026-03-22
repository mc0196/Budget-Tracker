namespace BudgetTracker.Domain.Entities;

/// <summary>
/// Represents a bank account from which statements are imported.
/// One account can have many transactions across many import batches.
/// </summary>
public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? BankName { get; private set; }
    public string Currency { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public ICollection<Transaction> Transactions { get; private set; } = [];

    private Account() { Name = null!; Currency = null!; } // EF Core constructor

    public Account(string name, string currency = "EUR", string? bankName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Currency = currency.ToUpperInvariant();
        BankName = bankName?.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
