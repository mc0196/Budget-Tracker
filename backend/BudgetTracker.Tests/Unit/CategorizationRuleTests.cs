using BudgetTracker.Domain.Entities;
using FluentAssertions;

namespace BudgetTracker.Tests.Unit;

public class CategorizationRuleTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Theory]
    [InlineData("netflix monthly charge", "netflix", true)]
    [InlineData("NETFLIX MONTHLY CHARGE", "netflix", true)]   // case-insensitive
    [InlineData("Netflix", "NETFLIX", true)]                  // keyword also case-insensitive
    [InlineData("spotify premium", "netflix", false)]
    [InlineData("", "netflix", false)]
    public void Matches_ReturnsExpectedResult(string description, string keyword, bool expected)
    {
        var rule = new CategorizationRule(keyword, CategoryId);

        rule.Matches(description).Should().Be(expected);
    }

    [Fact]
    public void Constructor_NormalizesKeywordToLowercase()
    {
        var rule = new CategorizationRule("NETFLIX", CategoryId);

        rule.Keyword.Should().Be("netflix");
    }

    [Fact]
    public void Constructor_WithEmptyKeyword_ThrowsArgumentException()
    {
        var act = () => new CategorizationRule("  ", CategoryId);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_PreventsMatching()
    {
        var rule = new CategorizationRule("netflix", CategoryId);
        rule.Deactivate();

        rule.Matches("netflix monthly").Should().BeFalse();
    }

    [Fact]
    public void Activate_RestoresMatching()
    {
        var rule = new CategorizationRule("netflix", CategoryId);
        rule.Deactivate();
        rule.Activate();

        rule.Matches("netflix monthly").Should().BeTrue();
    }
}
