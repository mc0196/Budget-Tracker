using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Interfaces;
using BudgetTracker.Domain.Enums;
using BudgetTracker.Domain.Exceptions;

namespace BudgetTracker.Application.Services;

/// <summary>
/// Handles CRUD operations on transactions after import.
/// Main use case: user manually fixing a category or description.
/// </summary>
public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);
        return transactions.Select(MapToDto).ToList();
    }

    public async Task<TransactionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        return transaction is null ? null : MapToDto(transaction);
    }

    public async Task<TransactionDto> UpdateAsync(
        Guid id,
        UpdateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Transaction {id} not found.");

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new DomainException($"Category {request.CategoryId} not found.");

            transaction.AssignCategory(category.Id);
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
            transaction.UpdateDescription(request.Description);

        if (request.Amount.HasValue)
            transaction.UpdateAmount(request.Amount.Value);

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(transaction);
    }

    public async Task<TransactionDto> CreateAsync(
        CreateTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, out var type))
            throw new DomainException($"Invalid transaction type '{request.Type}'. Use 'Income' or 'Expense'.");

        if (request.Amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        var defaultAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var transaction = new Domain.Entities.Transaction(
            accountId: defaultAccountId,
            date: request.Date,
            description: request.Description,
            amount: request.Amount,
            type: type,
            isManuallyCreated: true);

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, cancellationToken)
                ?? throw new DomainException($"Category {request.CategoryId} not found.");
            transaction.AssignCategory(category.Id);
        }

        await _transactionRepository.AddRangeAsync([transaction], cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);

        var saved = await _transactionRepository.GetByIdAsync(transaction.Id, cancellationToken);
        return MapToDto(saved!);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException($"Transaction {id} not found.");

        await _transactionRepository.DeleteAsync(transaction.Id, cancellationToken);
        await _transactionRepository.SaveChangesAsync(cancellationToken);
    }

    private static TransactionDto MapToDto(Domain.Entities.Transaction t) => new(
        Id: t.Id,
        AccountId: t.AccountId,
        CategoryId: t.CategoryId,
        CategoryName: t.Category?.Name,
        CategoryColor: t.Category?.Color,
        Date: t.Date,
        Description: t.Description,
        Amount: t.Amount,
        Type: t.Type,
        IsManuallyCreated: t.IsManuallyCreated,
        CreatedAt: t.CreatedAt
    );
}
