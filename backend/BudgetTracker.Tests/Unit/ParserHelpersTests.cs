using BudgetTracker.Infrastructure.Parsers;
using FluentAssertions;

namespace BudgetTracker.Tests.Unit;

/// <summary>
/// Tests for shared date and amount parsing logic.
/// These cover the real-world messiness of bank CSV exports.
/// </summary>
public class ParserHelpersTests
{
    // ── TryParseDate ─────────────────────────────────────────────────────────

    [Theory]
    [InlineData("15/01/2026", 2026, 1, 15)]   // dd/MM/yyyy (European)
    [InlineData("01/15/2026", 2026, 1, 15)]   // MM/dd/yyyy (US)
    [InlineData("2026-01-15", 2026, 1, 15)]   // yyyy-MM-dd (ISO)
    [InlineData("15.01.2026", 2026, 1, 15)]   // dd.MM.yyyy (German)
    [InlineData("5/1/2026",   2026, 1, 5)]    // d/M/yyyy   (short)
    public void TryParseDate_WithKnownFormat_ReturnsTrue(string input, int year, int month, int day)
    {
        var result = ParserHelpers.TryParseDate(input, out var date);

        result.Should().BeTrue();
        date.Should().Be(new DateOnly(year, month, day));
    }

    [Fact]
    public void TryParseDate_WithExcelSerialNumber_ParsesCorrectly()
    {
        // Excel serial 45658 = 2025-01-01
        var result = ParserHelpers.TryParseDate("45658", out var date);

        result.Should().BeTrue();
        date.Year.Should().Be(2025);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-date")]
    [InlineData("32/13/2026")]
    public void TryParseDate_WithInvalidInput_ReturnsFalse(string? input)
    {
        var result = ParserHelpers.TryParseDate(input, out _);

        result.Should().BeFalse();
    }

    // ── ParseAmount ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData("1234.56",    1234.56)]    // standard
    [InlineData("1,234.56",   1234.56)]    // US thousand separator
    [InlineData("1.234,56",   1234.56)]    // European thousand separator
    [InlineData("€1.234,56",  1234.56)]    // Euro symbol
    [InlineData("$1,234.56",  1234.56)]    // Dollar symbol
    [InlineData("£99.99",     99.99)]      // Pound symbol
    [InlineData("-50.00",     -50.00)]     // Negative (expense from bank)
    [InlineData("0",          0)]
    [InlineData("",           0)]
    [InlineData(null,         0)]
    public void ParseAmount_ReturnsCorrectDecimal(string? input, decimal expected)
    {
        ParserHelpers.ParseAmount(input).Should().Be(expected);
    }
}
