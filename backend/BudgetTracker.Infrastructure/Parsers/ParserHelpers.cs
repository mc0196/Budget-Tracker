using System.Globalization;

namespace BudgetTracker.Infrastructure.Parsers;

/// <summary>
/// Shared helpers for both CsvFileParser and ExcelFileParser.
/// Centralizes date and amount parsing so format rules are defined once.
/// </summary>
internal static class ParserHelpers
{
    private static readonly string[] DateFormats =
        ["dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd.MM.yyyy", "d/M/yyyy", "d-M-yyyy"];

    public static bool TryParseDate(string? raw, out DateOnly result)
    {
        if (string.IsNullOrWhiteSpace(raw)) { result = default; return false; }

        // Excel serial date (e.g. 45123.0)
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var serial)
            && serial is > 1 and < 2958466)
        {
            result = DateOnly.FromDateTime(DateTime.FromOADate(serial));
            return true;
        }

        foreach (var fmt in DateFormats)
        {
            if (DateOnly.TryParseExact(raw, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;
        }

        result = default;
        return false;
    }

    public static decimal ParseAmount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;

        // Remove currency symbols and whitespace in one pass
        var span = raw.AsSpan().Trim();
        Span<char> buffer = stackalloc char[span.Length];
        var writeIdx = 0;

        foreach (var ch in span)
        {
            if (ch is '€' or '$' or '£' or ' ') continue;
            buffer[writeIdx++] = ch;
        }

        var normalized = buffer[..writeIdx];
        var lastDot   = normalized.LastIndexOf('.');
        var lastComma = normalized.LastIndexOf(',');

        string forParsing;
        if (lastComma > lastDot)
        {
            // European format: 1.234,56 → 1234.56
            Span<char> tmp = stackalloc char[writeIdx];
            var tmpIdx = 0;
            foreach (var ch in normalized)
            {
                if (ch == '.') continue;
                tmp[tmpIdx++] = ch == ',' ? '.' : ch;
            }
            forParsing = tmp[..tmpIdx].ToString();
        }
        else
        {
            // US/standard format: remove commas
            Span<char> tmp = stackalloc char[writeIdx];
            var tmpIdx = 0;
            foreach (var ch in normalized)
            {
                if (ch != ',') tmp[tmpIdx++] = ch;
            }
            forParsing = tmp[..tmpIdx].ToString();
        }

        return decimal.TryParse(forParsing, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value : 0;
    }
}
