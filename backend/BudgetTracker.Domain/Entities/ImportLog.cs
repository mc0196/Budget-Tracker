namespace BudgetTracker.Domain.Entities;

/// <summary>
/// Tracks every file import. The FileHash (SHA-256) is used as an idempotency
/// key — if you accidentally upload the same statement twice, the API
/// rejects it instead of creating duplicate transactions.
/// </summary>
public class ImportLog
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string FileHash { get; private set; }
    public int RowsImported { get; private set; }
    public int RowsSkipped { get; private set; }
    public DateTimeOffset ImportedAt { get; private set; }

    private ImportLog() { FileName = null!; FileHash = null!; } // EF Core constructor

    public ImportLog(string fileName, string fileHash, int rowsImported, int rowsSkipped)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        FileHash = fileHash;
        RowsImported = rowsImported;
        RowsSkipped = rowsSkipped;
        ImportedAt = DateTimeOffset.UtcNow;
    }
}
