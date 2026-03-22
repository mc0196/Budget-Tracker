using System.Data;
using System.Text;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Infrastructure.Parsers;

/// <summary>
/// Parses XLS and XLSX bank statements using ExcelDataReader.
/// ExcelDataReader reads legacy and modern Excel formats without requiring Office installed.
/// </summary>
public class ExcelFileParser : IFileParser
{
    // RegisterProvider is idempotent global state — run it once per process, not per instance.
    static ExcelFileParser() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private readonly ILogger<ExcelFileParser> _logger;

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
        using var reader  = ExcelReaderFactory.CreateReader(fileStream);
        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });

        if (dataSet.Tables.Count == 0)
            return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>([]);

        var table = dataSet.Tables[0];
        _logger.LogDebug("Excel sheet '{Sheet}' has {Rows} rows, {Cols} columns.",
            table.TableName, table.Rows.Count, table.Columns.Count);

        var columnNames = table.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToArray();

        var results = new List<ParsedTransactionRow>();

        foreach (DataRow row in table.Rows)
        {
            try
            {
                var parsed = ParseRow(row, columnNames);
                if (parsed is not null)
                    results.Add(parsed);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping malformed Excel row.");
            }
        }

        return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>(results);
    }

    private static ParsedTransactionRow? ParseRow(DataRow row, string[] columnNames)
    {
        var dateValue = GetCellValue(row, columnNames, ["Date", "Transaction Date", "ValueDate", "Data"]);
        if (!ParserHelpers.TryParseDate(dateValue, out var date))
            return null;

        var description = GetCellValue(row, columnNames, ["Description", "Narrative", "Details", "Memo", "Reference"])
            ?? "No description";

        if (string.IsNullOrWhiteSpace(description))
            return null;

        decimal amount;
        var amountRaw = GetCellValue(row, columnNames, ["Amount", "Value", "Importo"]);
        if (amountRaw is not null)
        {
            amount = ParserHelpers.ParseAmount(amountRaw);
        }
        else
        {
            var debit  = ParserHelpers.ParseAmount(GetCellValue(row, columnNames, ["Debit",  "Withdrawal"]));
            var credit = ParserHelpers.ParseAmount(GetCellValue(row, columnNames, ["Credit", "Deposit"]));
            // Debits reduce balance (expense), credits increase it (income)
            amount = credit - debit;
        }

        var originalText = string.Join(" | ", row.ItemArray.Select(v => v?.ToString() ?? ""));

        return new ParsedTransactionRow(date, description.Trim(), amount, originalText);
    }

    private static string? GetCellValue(DataRow row, string[] columnNames, string[] candidates)
    {
        var column = columnNames.FirstOrDefault(c => candidates.Contains(c, StringComparer.OrdinalIgnoreCase));
        if (column is null) return null;
        var value = row[column];
        return value == DBNull.Value ? null : value?.ToString();
    }
}
