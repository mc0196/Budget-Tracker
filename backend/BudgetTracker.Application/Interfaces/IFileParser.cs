using BudgetTracker.Application.DTOs;

namespace BudgetTracker.Application.Interfaces;

/// <summary>
/// Abstraction over file parsing. Two implementations exist:
/// CsvFileParser and ExcelFileParser (in Infrastructure).
/// The Application layer only depends on this interface.
/// </summary>
public interface IFileParser
{
    bool CanParse(string fileName);
    Task<IReadOnlyList<ParsedTransactionRow>> ParseAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
