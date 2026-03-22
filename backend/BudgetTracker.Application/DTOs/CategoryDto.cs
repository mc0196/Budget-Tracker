using BudgetTracker.Domain.Enums;

namespace BudgetTracker.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    TransactionType Type,
    string? Color,
    string? Icon
);

public record CreateCategoryRequest(
    string Name,
    TransactionType Type,
    string? Color,
    string? Icon
);
