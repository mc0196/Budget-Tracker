using System.Security.Cryptography;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using BudgetTracker.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Application.Services;

/// <summary>
/// Orchestrates the full import flow:
/// 1. Compute file hash → reject duplicates early.
/// 2. Resolve the correct parser (CSV vs Excel) via the strategy pattern.
/// 3. Parse raw rows from the file.
/// 4. Convert rows to Transaction entities, applying type inference.
/// 5. Auto-categorize via keyword rules.
/// 6. Persist everything in a single unit of work.
/// 7. Record the import log for idempotency.
/// </summary>
public class TransactionImportService
{
    private readonly IEnumerable<IFileParser> _parsers;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IImportLogRepository _importLogRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly CategorizationService _categorizationService;
    private readonly ILogger<TransactionImportService> _logger;

    private static readonly Guid DefaultAccountId = new("00000000-0000-0000-0000-000000000001");

    // Palette for auto-created categories. Colors cycle if there are more categories than entries.
    private static readonly string[] CategoryColors =
    [
        "#f97316", "#3b82f6", "#22c55e", "#ec4899", "#a855f7",
        "#14b8a6", "#ef4444", "#f59e0b", "#84cc16", "#0ea5e9",
        "#e879f9", "#fb923c", "#34d399", "#60a5fa", "#f472b6",
    ];

    public TransactionImportService(
        IEnumerable<IFileParser> parsers,
        ITransactionRepository transactionRepository,
        IImportLogRepository importLogRepository,
        ICategoryRepository categoryRepository,
        CategorizationService categorizationService,
        ILogger<TransactionImportService> logger)
    {
        _parsers = parsers;
        _transactionRepository = transactionRepository;
        _importLogRepository = importLogRepository;
        _categoryRepository = categoryRepository;
        _categorizationService = categorizationService;
        _logger = logger;
    }

    public async Task<ImportResultDto> ImportAsync(
        Stream fileStream,
        string fileName,
        Guid? accountId = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedAccountId = accountId ?? DefaultAccountId;

        // Step 1: Compute file hash using a buffered copy (stream may not be seekable)
        var fileBytes = await ReadStreamAsync(fileStream, cancellationToken);
        var fileHash = ComputeSha256(fileBytes);

        // Step 2: Idempotency check
        if (await _importLogRepository.ExistsByHashAsync(fileHash, cancellationToken))
        {
            _logger.LogWarning("Duplicate file detected: {FileName} (hash: {Hash})", fileName, fileHash);
            return new ImportResultDto(0, 0, "This file has already been imported.", IsDuplicate: true);
        }

        // Step 3: Resolve parser
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName))
            ?? throw new NotSupportedException($"No parser supports file '{fileName}'. Supported formats: CSV, XLS, XLSX.");

        // Step 4: Parse rows
        using var memoryStream = new MemoryStream(fileBytes);
        var parsedRows = await parser.ParseAsync(memoryStream, cancellationToken);

        _logger.LogInformation("Parsed {Count} rows from '{FileName}'.", parsedRows.Count, fileName);

        // Step 5: Convert to domain entities, applying categories from the file where available
        var (transactions, skippedCount) = await BuildTransactionsAsync(parsedRows, resolvedAccountId, fileName, cancellationToken);

        // Step 6: Auto-categorize uncategorized transactions via keyword rules
        await _categorizationService.ApplyRulesAsync(
            transactions.Where(t => t.CategoryId is null).ToList(), cancellationToken);

        // Step 7: Persist
        await _transactionRepository.AddRangeAsync(transactions, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        // Step 8: Record import log
        var importLog = new ImportLog(fileName, fileHash, transactions.Count, skippedCount);
        await _importLogRepository.AddAsync(importLog, cancellationToken);
        await _importLogRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Import complete: {Imported} imported, {Skipped} skipped from '{FileName}'.",
            transactions.Count, skippedCount, fileName);

        return new ImportResultDto(
            transactions.Count,
            skippedCount,
            $"Successfully imported {transactions.Count} transactions.");
    }

    private async Task<(List<Transaction> Transactions, int SkippedCount)> BuildTransactionsAsync(
        IReadOnlyList<ParsedTransactionRow> rows,
        Guid accountId,
        string fileName,
        CancellationToken cancellationToken)
    {
        // Build a cache of all existing categories (name → id, case-insensitive).
        // New categories found in the file will be added here and persisted.
        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        var categoryCache = allCategories.ToDictionary(
            c => c.Name.Trim().ToLowerInvariant(), c => c.Id);

        var transactions = new List<Transaction>(rows.Count);
        var skippedCount = 0;

        foreach (var row in rows)
        {
            if (row.Amount == 0)
            {
                skippedCount++;
                continue;
            }

            var absoluteAmount = Math.Abs(row.Amount);
            var type = row.Amount < 0 ? TransactionType.Expense : TransactionType.Income;

            try
            {
                var transaction = new Transaction(
                    accountId: accountId,
                    date: row.Date,
                    description: row.Description,
                    amount: absoluteAmount,
                    type: type,
                    originalText: row.OriginalText,
                    sourceFile: fileName);

                // Assign category from the file if present
                if (!string.IsNullOrWhiteSpace(row.CategoryName))
                {
                    var key = row.CategoryName.Trim().ToLowerInvariant();
                    if (!categoryCache.TryGetValue(key, out var categoryId))
                    {
                        // Category not yet known — create it with a distinct color from the palette
                        var color = CategoryColors[categoryCache.Count % CategoryColors.Length];
                        var newCategory = new Category(row.CategoryName.Trim(), type, color);
                        await _categoryRepository.AddAsync(newCategory, cancellationToken);
                        await _categoryRepository.SaveChangesAsync(cancellationToken);
                        categoryCache[key] = newCategory.Id;
                        categoryId = newCategory.Id;
                        _logger.LogInformation("Created new category '{Name}' ({Color}) from import.", newCategory.Name, color);
                    }
                    transaction.AssignCategory(categoryId);
                }

                transactions.Add(transaction);
            }
            catch (Exception ex)
            {
                skippedCount++;
                _logger.LogDebug(ex, "Skipped row with description '{Description}'.", row.Description);
            }
        }

        return (transactions, skippedCount);
    }

    private static async Task<byte[]> ReadStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        return ms.ToArray();
    }

    private static string ComputeSha256(byte[] data)
    {
        var hash = SHA256.HashData(data);
        return Convert.ToHexStringLower(hash);
    }
}
