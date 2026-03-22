using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure.Persistence.Repositories;

public class ImportLogRepository : IImportLogRepository
{
    private readonly AppDbContext _context;

    public ImportLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByHashAsync(string fileHash, CancellationToken cancellationToken = default)
        => await _context.ImportLogs.AnyAsync(l => l.FileHash == fileHash, cancellationToken);

    public async Task AddAsync(ImportLog log, CancellationToken cancellationToken = default)
        => await _context.ImportLogs.AddAsync(log, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
