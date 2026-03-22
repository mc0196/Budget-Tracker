using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface ICategorizationRuleRepository
{
    /// <summary>Returns all active rules ordered by descending priority.</summary>
    Task<IReadOnlyList<CategorizationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default);
    Task AddAsync(CategorizationRule rule, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
