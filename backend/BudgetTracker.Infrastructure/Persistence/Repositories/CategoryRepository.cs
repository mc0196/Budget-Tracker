using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Categories.FindAsync([id], cancellationToken);

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
        => await _context.Categories.AddAsync(category, cancellationToken);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
