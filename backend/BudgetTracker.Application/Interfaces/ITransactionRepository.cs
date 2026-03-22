using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Application.Interfaces;

/// <summary>
/// Port for transaction persistence. The Application layer depends on this
/// interface; the Infrastructure layer provides the concrete EF Core implementation.
/// This inversion is the "D" in SOLID and the heart of Clean Architecture.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByPeriodAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByCategoryAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Transaction> transactions, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
