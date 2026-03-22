using System.Globalization;
using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Infrastructure.Parsers;

/// <summary>
/// Parses CSV bank statements using CsvHelper.
///
/// Real-world challenge: every bank exports a slightly different CSV format.
/// This parser handles the most common patterns:
///   - Column names may vary: "Date", "Transaction Date", "ValueDate"
///   - Amount may be split into "Debit"/"Credit" columns, or a single signed "Amount" column
///   - Decimal separators may be '.' or ','
///   - Date formats vary by locale
///
/// Strategy: try known column name patterns; fall back to position-based parsing.
/// </summary>
public class CsvFileParser : IFileParser
{
    private readonly ILogger<CsvFileParser> _logger;

    private static readonly string[] DateColumnNames =
        ["Date", "Transaction Date", "ValueDate", "Booking Date", "Data", "Datum"];
    private static readonly string[] DescriptionColumnNames =
        ["Description", "Narrative", "Details", "Memo", "Reference", "Descrizione"];
    private static readonly string[] AmountColumnNames =
        ["Amount", "Value", "Importo", "Betrag"];
    private static readonly string[] DebitColumnNames =
        ["Debit", "Withdrawal", "Uscite", "Soll"];
    private static readonly string[] CreditColumnNames =
        ["Credit", "Deposit", "Entrate", "Haben"];

    public CsvFileParser(ILogger<CsvFileParser> logger)
    {
        _logger = logger;
    }

    public bool CanParse(string fileName)
        => fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);

    public Task<IReadOnlyList<ParsedTransactionRow>> ParseAsync(
        Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
        };

        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();

        var headers = csv.HeaderRecord ?? [];
        _logger.LogDebug("CSV headers detected: {Headers}", string.Join(", ", headers));

        var dateColumn        = FindColumn(headers, DateColumnNames);
        var descriptionColumn = FindColumn(headers, DescriptionColumnNames);
        var amountColumn      = FindColumn(headers, AmountColumnNames);
        var debitColumn       = FindColumn(headers, DebitColumnNames);
        var creditColumn      = FindColumn(headers, CreditColumnNames);

        var results = new List<ParsedTransactionRow>();

        while (csv.Read())
        {
            try
            {
                var row = ParseRow(csv, headers.Length, dateColumn, descriptionColumn, amountColumn, debitColumn, creditColumn);
                if (row is not null)
                    results.Add(row);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping malformed row at line {Line}", csv.CurrentIndex);
            }
        }

        _logger.LogInformation("CSV parser extracted {Count} rows.", results.Count);
        return Task.FromResult<IReadOnlyList<ParsedTransactionRow>>(results);
    }

    private static ParsedTransactionRow? ParseRow(
        CsvReader csv,
        int columnCount,
        string? dateColumn,
        string? descriptionColumn,
        string? amountColumn,
        string? debitColumn,
        string? creditColumn)
    {
        var dateRaw = dateColumn is not null ? csv.GetField(dateColumn) ?? "" : csv.GetField(0) ?? "";
        if (!ParserHelpers.TryParseDate(dateRaw, out var date))
            return null;

        var description = (descriptionColumn is not null ? csv.GetField(descriptionColumn) : csv.GetField(1))
            ?? "No description";

        if (string.IsNullOrWhiteSpace(description))
            return null;

        decimal amount;
        if (amountColumn is not null)
        {
            amount = ParserHelpers.ParseAmount(csv.GetField(amountColumn));
        }
        else if (debitColumn is not null || creditColumn is not null)
        {
            var debit  = debitColumn  is not null ? ParserHelpers.ParseAmount(csv.GetField(debitColumn))  : 0;
            var credit = creditColumn is not null ? ParserHelpers.ParseAmount(csv.GetField(creditColumn)) : 0;
            // Debits reduce balance (expense), credits increase it (income)
            amount = credit - debit;
        }
        else
        {
            amount = ParserHelpers.ParseAmount(csv.GetField(2));
        }

        // Capture raw line using actual column count (not a hardcoded 10)
        var originalText = string.Join("|", Enumerable.Range(0, columnCount)
            .Select(i => csv.TryGetField<string>(i, out var val) ? val : null)
            .Where(v => v is not null));

        return new ParsedTransactionRow(date, description.Trim(), amount, originalText);
    }

    private static string? FindColumn(string[] headers, string[] candidates)
        => headers.FirstOrDefault(h => candidates.Contains(h, StringComparer.OrdinalIgnoreCase));
}
