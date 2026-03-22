namespace BudgetTracker.Application.DTOs;

public record ImportResultDto(
    int RowsImported,
    int RowsSkipped,
    string Message,
    bool IsDuplicate = false
);
