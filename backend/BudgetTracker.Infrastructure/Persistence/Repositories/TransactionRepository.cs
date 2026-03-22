using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByPeriodAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Date >= from && t.Date <= to)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default)
        => await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);

    public async Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default)
        => await _context.Transactions.AddRangeAsync(transactions, cancellationToken);

    public Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Transactions.FindAsync([id], cancellationToken);
        if (transaction is not null)
            _context.Transactions.Remove(transaction);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
