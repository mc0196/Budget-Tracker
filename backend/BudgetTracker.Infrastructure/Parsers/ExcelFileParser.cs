using System.Data;
using System.Text;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Infrastructure.Parsers;

/// <summary>
/// Parses XLS and XLSX bank statements using ExcelDataReader.
/// Handles files with metadata rows before the actual data table (e.g. Intesa Sanpaolo).
/// </summary>
public class ExcelFileParser : IFileParser
{
    // RegisterProvider is idempotent global state — run it once per process, not per instance.
    static ExcelFileParser() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private readonly ILogger<ExcelFileParser> _logger;

    // All known column name candidates — used to detect the real header row.
    private static readonly HashSet<string> KnownColumnNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Date", "Transaction Date", "ValueDate", "Data",
        "Description", "Narrative", "Details", "Memo", "Reference", "Operazione", "Dettagli",
        "Amount", "Value", "Importo", "Debit", "Credit", "Withdrawal", "Deposit"
    };

    public ExcelFileParser(ILogger<ExcelFileParser> logger)
    {
        _logger = logger;
    }

    public bool CanParse(string fileName)
        => fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
        || fileName.EndsWith(".xls",  StringComparison.OrdinalIgnoreCase);

    public Task<IReadOnlyList<ParsedTransactionRow>> ParseAsync(
        Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = ExcelReaderFactory.CreateReader(fileStream);

        // Read ALL rows as plain data so we can scan for the real header row.
        // Some banks (e.g. Intesa Sanpaolo) prepend 20+ metadata rows before the table.
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = false }
        });

        if (dataSet.Tables.Count == 0)
            return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>([]);

        var table = dataSet.Tables[0];
        _logger.LogDebug("Excel sheet '{Sheet}' has {Rows} rows, {Cols} columns.",
            table.TableName, table.Rows.Count, table.Columns.Count);

        // Find the first row where at least 2 cells match known column names.
        // This is the real header row.
        int headerRowIndex = -1;
        string[] headerCells = [];

        for (int i = 0; i < table.Rows.Count; i++)
        {
            var cells = table.Rows[i].ItemArray
                .Select(v => v?.ToString()?.Trim() ?? "")
                .ToArray();

            var matchCount = cells.Count(c => KnownColumnNames.Contains(c));
            if (matchCount >= 2)
            {
                headerRowIndex = i;
                headerCells = cells;
                _logger.LogInformation("Excel: found header row at index {Index}: [{Columns}]",
                    i, string.Join(", ", cells.Where(c => !string.IsNullOrEmpty(c))));
                break;
            }
        }

        if (headerRowIndex < 0)
        {
            _logger.LogWarning("Excel: could not find a header row after scanning {Rows} rows.", table.Rows.Count);
            return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>([]);
        }

        var results = new List<ParsedTransactionRow>();

        for (int i = headerRowIndex + 1; i < table.Rows.Count; i++)
        {
            try
            {
                var parsed = ParseRow(table.Rows[i], headerCells);
                if (parsed is not null)
                    results.Add(parsed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Excel: skipping malformed row at index {Index}.", i);
            }
        }

        return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>(results);
    }

    private static ParsedTransactionRow? ParseRow(DataRow row, string[] headerCells)
    {
        var dateValue = GetCellByHeader(row, headerCells, ["Date", "Transaction Date", "ValueDate", "Data"]);
        if (!ParserHelpers.TryParseDate(dateValue, out var date))
            return null;

        var description = GetCellByHeader(row, headerCells,
            ["Description", "Narrative", "Details", "Memo", "Reference", "Operazione", "Dettagli"])
            ?? "No description";

        if (string.IsNullOrWhiteSpace(description))
            return null;

        decimal amount;
        var amountRaw = GetCellByHeader(row, headerCells, ["Amount", "Value", "Importo"]);
        if (amountRaw is not null)
        {
            amount = ParserHelpers.ParseAmount(amountRaw);
        }
        else
        {
            var debit  = ParserHelpers.ParseAmount(GetCellByHeader(row, headerCells, ["Debit",  "Withdrawal"]));
            var credit = ParserHelpers.ParseAmount(GetCellByHeader(row, headerCells, ["Credit", "Deposit"]));
            amount = credit - debit;
        }

        var originalText = string.Join(" | ", row.ItemArray.Select(v => v?.ToString() ?? ""));

        return new ParsedTransactionRow(date, description.Trim(), amount, originalText);
    }

    /// <summary>
    /// Finds the column index by matching header cells against known candidates,
    /// then returns the cell value at that index from the data row.
    /// This is needed because with UseHeaderRow=false, DataTable columns are named
    /// Column0, Column1... so we must use index-based access.
    /// </summary>
    private static string? GetCellByHeader(DataRow row, string[] headerCells, string[] candidates)
    {
        var colIndex = Array.FindIndex(headerCells,
            c => candidates.Contains(c, StringComparer.OrdinalIgnoreCase));

        if (colIndex < 0 || colIndex >= row.ItemArray.Length) return null;

        var value = row.ItemArray[colIndex];
        if (value == DBNull.Value || value is null) return null;

        // ExcelDataReader converts date-formatted cells to DateTime objects.
        // Format explicitly so TryParseDate can handle it.
        if (value is DateTime dt)
            return dt.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

        return value.ToString()?.Trim();
    }
}
