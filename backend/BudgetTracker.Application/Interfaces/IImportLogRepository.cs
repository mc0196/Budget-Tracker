using BudgetTracker.Domain.Entities;

namespace BudgetTracker.Application.Interfaces;

public interface IImportLogRepository
{
    Task<bool> ExistsByHashAsync(string fileHash, CancellationToken cancellationToken = default);
    Task AddAsync(ImportLog log, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
