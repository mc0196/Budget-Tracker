using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Domain.Entities;

/// <summary>
/// Represents a classification label for transactions (e.g. Groceries, Salary).
/// Categories are user-definable and carry a type (Income/Expense) so the UI
/// can filter them contextually.
/// </summary>
public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Color { get; private set; }   // hex e.g. "#f97316"
    public string? Icon { get; private set; }    // icon name/emoji

    public ICollection<Transaction> Transactions { get; private set; } = [];
    public ICollection<CategorizationRule> Rules { get; private set; } = [];

    private Category() { Name = null!; } // EF Core constructor

    public Category(string name, TransactionType type, string? color = null, string? icon = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        Id = Guid.NewGuid();
        Name = name.Trim();
        Type = type;
        Color = color;
        Icon = icon;
    }

    public void Update(string name, string? color, string? icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty.", nameof(name));

        Name = name.Trim();
        Color = color;
        Icon = icon;
    }
}
