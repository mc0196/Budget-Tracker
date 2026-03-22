using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure.Persistence.Repositories;

public class CategorizationRuleRepository : ICategorizationRuleRepository
{
    private readonly AppDbContext _context;

    public CategorizationRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategorizationRule>> GetActiveRulesAsync(CancellationToken cancellationToken = default)
        => await _context.CategorizationRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CategorizationRule rule, CancellationToken cancellationToken = default)
        => await _context.CategorizationRules.AddAsync(rule, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
