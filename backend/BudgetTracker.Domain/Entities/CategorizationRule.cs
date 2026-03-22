namespace BudgetTracker.Domain.Entities;

/// <summary>
/// A keyword-based rule that auto-assigns a category to transactions
/// whose description contains the keyword. Rules are evaluated in
/// descending Priority order — the first match wins.
/// </summary>
public class CategorizationRule
{
    public Guid Id { get; private set; }
    public string Keyword { get; private set; }
    public Guid CategoryId { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }

    public Category Category { get; private set; } = null!;

    private CategorizationRule() { Keyword = null!; } // EF Core constructor

    public CategorizationRule(string keyword, Guid categoryId, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            throw new ArgumentException("Keyword cannot be empty.", nameof(keyword));

        Id = Guid.NewGuid();
        Keyword = keyword.Trim().ToLowerInvariant();
        CategoryId = categoryId;
        Priority = priority;
        IsActive = true;
    }

    public bool Matches(string transactionDescription)
        => IsActive && transactionDescription.Contains(Keyword, StringComparison.OrdinalIgnoreCase);

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
