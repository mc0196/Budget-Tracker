using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Application.DTOs;

/// <summary>Response DTO for a single transaction. Never expose domain entities directly
/// over the API — DTOs let you evolve the API contract independently from the domain.</summary>
public record TransactionDto(
    Guid Id,
    Guid AccountId,
    Guid? CategoryId,
    string? CategoryName,
    string? CategoryColor,
    DateOnly Date,
    string Description,
    decimal Amount,
    TransactionType Type,
    bool IsManuallyCreated,
    DateTimeOffset CreatedAt
);

public record UpdateTransactionRequest(
    Guid? CategoryId,
    string? Description,
    decimal? Amount
);
