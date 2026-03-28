namespace BudgetTracker.Application.DTOs;

/// <summary>
/// Raw data extracted by a file parser before any business logic is applied.
/// This is an intermediate DTO — not a domain entity, not a response object.
/// It represents "what the file said" before we decide what it means.
/// </summary>
public record ParsedTransactionRow(
    DateOnly Date,
    string Description,
    decimal Amount,
    string OriginalText,
    string? CategoryName = null
);
