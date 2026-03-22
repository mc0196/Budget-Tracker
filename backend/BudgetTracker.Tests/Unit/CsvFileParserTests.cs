using System.Text;
using BudgetTracker.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace BudgetTracker.Tests.Unit;

/// <summary>
/// Tests for CsvFileParser. Each test uses an inline CSV string to simulate
/// a real bank export without touching the filesystem.
/// </summary>
public class CsvFileParserTests
{
    private readonly CsvFileParser _parser = new(NullLogger<CsvFileParser>.Instance);

    private static Stream ToStream(string csv)
        => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public void CanParse_ReturnsTrueForCsvExtension()
    {
        _parser.CanParse("statement.csv").Should().BeTrue();
        _parser.CanParse("statement.CSV").Should().BeTrue();
        _parser.CanParse("statement.xlsx").Should().BeFalse();
    }

    [Fact]
    public async Task ParseAsync_StandardFormat_ParsesAllRows()
    {
        var csv = """
            Date,Description,Amount
            15/01/2026,Salary January,2500.00
            16/01/2026,Netflix,-15.99
            17/01/2026,Supermarket Carrefour,-87.40
            """;

        var rows = await _parser.ParseAsync(ToStream(csv));

        rows.Should().HaveCount(3);
        rows[0].Description.Should().Be("Salary January");
        rows[0].Amount.Should().Be(2500.00m);
        rows[1].Amount.Should().Be(-15.99m);
    }

    [Fact]
    public async Task ParseAsync_DebitCreditColumns_ComputesSignedAmount()
    {
        var csv = """
            Date,Description,Debit,Credit
            15/01/2026,Salary,,2500.00
            16/01/2026,Netflix,15.99,
            """;

        var rows = await _parser.ParseAsync(ToStream(csv));

        rows.Should().HaveCount(2);
        rows[0].Amount.Should().Be(2500.00m);   // credit: positive
        rows[1].Amount.Should().Be(-15.99m);    // debit: negative
    }

    [Fact]
    public async Task ParseAsync_EuropeanDecimalFormat_ParsesCorrectly()
    {
        // Amounts with European separators must be quoted in CSV to avoid being split into two columns
        var csv = """
            Date,Description,Amount
            15/01/2026,Affitto,"-1.250,00"
            """;

        // European: 1.250,00 = 1250.00
        var rows = await _parser.ParseAsync(ToStream(csv));

        rows.Should().HaveCount(1);
        rows[0].Amount.Should().Be(-1250.00m);
    }

    [Fact]
    public async Task ParseAsync_RowWithUnparseableDate_SkipsRow()
    {
        var csv = """
            Date,Description,Amount
            NOT-A-DATE,Bad row,100.00
            15/01/2026,Good row,200.00
            """;

        var rows = await _parser.ParseAsync(ToStream(csv));

        rows.Should().HaveCount(1);
        rows[0].Description.Should().Be("Good row");
    }

    [Fact]
    public async Task ParseAsync_EmptyFile_ReturnsEmptyList()
    {
        var csv = "Date,Description,Amount\n";

        var rows = await _parser.ParseAsync(ToStream(csv));

        rows.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_PreservesOriginalText()
    {
        var csv = """
            Date,Description,Amount
            15/01/2026,Salary,2500.00
            """;

        var rows = await _parser.ParseAsync(ToStream(csv));

        rows[0].OriginalText.Should().NotBeNullOrEmpty();
    }
}
