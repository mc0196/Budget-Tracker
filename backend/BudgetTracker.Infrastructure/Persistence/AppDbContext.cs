using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BudgetTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CategorizationRule> CategorizationRules => Set<CategorizationRule>();
    public DbSet<ImportLog> ImportLogs => Set<ImportLog>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureAccount(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureTransaction(modelBuilder);
        ConfigureCategorizationRule(modelBuilder);
        ConfigureImportLog(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.BankName).HasMaxLength(100);
            entity.Property(a => a.Currency).IsRequired().HasMaxLength(3);
        });
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.Name).IsUnique();
            entity.Property(c => c.Type).HasConversion<string>();
            entity.Property(c => c.Color).HasMaxLength(7);
            entity.Property(c => c.Icon).HasMaxLength(50);
        });
    }

    private static void ConfigureTransaction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).IsRequired();
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Type).HasConversion<string>();
            entity.Property(t => t.SourceFile).HasMaxLength(255);

            entity.HasOne(t => t.Account)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(t => t.AccountId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.Date);
            entity.HasIndex(t => t.AccountId);
            entity.HasIndex(t => t.CategoryId);
        });
    }

    private static void ConfigureCategorizationRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CategorizationRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Keyword).IsRequired().HasMaxLength(200);

            entity.HasOne(r => r.Category)
                  .WithMany(c => c.Rules)
                  .HasForeignKey(r => r.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureImportLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ImportLog>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.FileName).IsRequired().HasMaxLength(255);
            entity.Property(l => l.FileHash).IsRequired().HasMaxLength(64);
            entity.HasIndex(l => l.FileHash).IsUnique();
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // All GUIDs and dates are hardcoded — EF Core requires deterministic values
        // in HasData so that migrations don't change on every build.
        var seededAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        modelBuilder.Entity<Account>().HasData(new[]
        {
            new { Id = new Guid("00000000-0000-0000-0000-000000000001"),
                  Name = "My Bank Account", BankName = (string?)null,
                  Currency = "EUR", CreatedAt = seededAt }
        });

        var categories = new[]
        {
            new { Id = new Guid("10000000-0000-0000-0000-000000000001"), Name = "Salary",
                  Type = TransactionType.Income,  Color = (string?)"#22c55e", Icon = (string?)"💰" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000002"), Name = "Freelance",
                  Type = TransactionType.Income,  Color = (string?)"#84cc16", Icon = (string?)"💻" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000003"), Name = "Groceries",
                  Type = TransactionType.Expense, Color = (string?)"#f97316", Icon = (string?)"🛒" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000004"), Name = "Transport",
                  Type = TransactionType.Expense, Color = (string?)"#3b82f6", Icon = (string?)"🚌" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000005"), Name = "Utilities",
                  Type = TransactionType.Expense, Color = (string?)"#a855f7", Icon = (string?)"⚡" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000006"), Name = "Entertainment",
                  Type = TransactionType.Expense, Color = (string?)"#ec4899", Icon = (string?)"🎬" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000007"), Name = "Health",
                  Type = TransactionType.Expense, Color = (string?)"#14b8a6", Icon = (string?)"💊" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000008"), Name = "Dining",
                  Type = TransactionType.Expense, Color = (string?)"#ef4444", Icon = (string?)"🍽️" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000009"), Name = "Shopping",
                  Type = TransactionType.Expense, Color = (string?)"#f59e0b", Icon = (string?)"🛍️" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000010"), Name = "Other",
                  Type = TransactionType.Expense, Color = (string?)"#6b7280", Icon = (string?)null },
            new { Id = new Guid("10000000-0000-0000-0000-000000000011"), Name = "Investments",
                  Type = TransactionType.Expense, Color = (string?)"#0ea5e9", Icon = (string?)"📈" },
            new { Id = new Guid("10000000-0000-0000-0000-000000000012"), Name = "Savings",
                  Type = TransactionType.Expense, Color = (string?)"#8b5cf6", Icon = (string?)"🏦" },
        };
        modelBuilder.Entity<Category>().HasData(categories);

        var rules = new[]
        {
            new { Id = new Guid("20000000-0000-0000-0000-000000000001"), Keyword = "salary",      CategoryId = new Guid("10000000-0000-0000-0000-000000000001"), Priority = 10, IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000002"), Keyword = "payroll",     CategoryId = new Guid("10000000-0000-0000-0000-000000000001"), Priority = 10, IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000003"), Keyword = "supermarket", CategoryId = new Guid("10000000-0000-0000-0000-000000000003"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000004"), Keyword = "grocery",     CategoryId = new Guid("10000000-0000-0000-0000-000000000003"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000005"), Keyword = "uber",        CategoryId = new Guid("10000000-0000-0000-0000-000000000004"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000006"), Keyword = "metro",       CategoryId = new Guid("10000000-0000-0000-0000-000000000004"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000007"), Keyword = "netflix",     CategoryId = new Guid("10000000-0000-0000-0000-000000000006"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000008"), Keyword = "spotify",     CategoryId = new Guid("10000000-0000-0000-0000-000000000006"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000009"), Keyword = "pharmacy",    CategoryId = new Guid("10000000-0000-0000-0000-000000000007"), Priority = 5,  IsActive = true },
            new { Id = new Guid("20000000-0000-0000-0000-000000000010"), Keyword = "restaurant",  CategoryId = new Guid("10000000-0000-0000-0000-000000000008"), Priority = 5,  IsActive = true },
        };
        modelBuilder.Entity<CategorizationRule>().HasData(rules);
    }
}
