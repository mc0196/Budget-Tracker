using BudgetTracker.Application.Interfaces;
using BudgetTracker.Application.Services;
using BudgetTracker.Infrastructure.Parsers;
using BudgetTracker.Infrastructure.Persistence;
using BudgetTracker.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Infrastructure;

/// <summary>
/// Extension method that registers all Infrastructure services.
/// Calling code (Program.cs) only imports this; it doesn't know about EF Core,
/// repositories, or parsers directly. This is the "composition root" pattern.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database — SQLite for dev simplicity, swap connection string for PostgreSQL in prod
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=budget_tracker.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IImportLogRepository, ImportLogRepository>();
        services.AddScoped<ICategorizationRuleRepository, CategorizationRuleRepository>();

        // File parsers — registered as IEnumerable<IFileParser>
        // TransactionImportService picks the right one based on file extension
        services.AddScoped<IFileParser, CsvFileParser>();
        services.AddScoped<IFileParser, ExcelFileParser>();

        // Application services
        services.AddScoped<CategorizationService>();
        services.AddScoped<TransactionImportService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<DashboardService>();

        return services;
    }

    /// <summary>
    /// Applies pending EF Core migrations on startup.
    /// Call this from Program.cs after building the app.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
