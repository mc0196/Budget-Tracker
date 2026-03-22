using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BudgetTracker.Application.Services;

/// <summary>
/// Applies keyword-based categorization rules to a list of transactions.
/// Rules are loaded once per import batch (not per transaction) for efficiency.
///
/// How it works:
/// 1. Load all active rules sorted by descending priority.
/// 2. For each transaction, find the first matching rule.
/// 3. Assign its category. Unmatched transactions stay uncategorized.
///
/// You can extend this later with an AI-based classifier without
/// changing any other part of the application.
/// </summary>
public class CategorizationService
{
    private readonly ICategorizationRuleRepository _ruleRepository;
    private readonly ILogger<CategorizationService> _logger;

    public CategorizationService(
        ICategorizationRuleRepository ruleRepository,
        ILogger<CategorizationService> logger)
    {
        _ruleRepository = ruleRepository;
        _logger = logger;
    }

    public async Task ApplyRulesAsync(
        IReadOnlyList<Transaction> transactions,
        CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.GetActiveRulesAsync(cancellationToken);

        if (rules.Count == 0)
        {
            _logger.LogInformation("No active categorization rules found. Skipping auto-categorization.");
            return;
        }

        var categorized = 0;

        foreach (var transaction in transactions)
        {
            var matchingRule = rules.FirstOrDefault(rule => rule.Matches(transaction.Description));

            if (matchingRule is not null)
            {
                transaction.AssignCategory(matchingRule.CategoryId);
                categorized++;
                _logger.LogDebug(
                    "Transaction '{Description}' matched rule '{Keyword}' → category {CategoryId}",
                    transaction.Description, matchingRule.Keyword, matchingRule.CategoryId);
            }
        }

        _logger.LogInformation(
            "Auto-categorized {Categorized} out of {Total} transactions.",
            categorized, transactions.Count);
    }
}
