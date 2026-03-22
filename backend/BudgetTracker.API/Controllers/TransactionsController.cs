using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>Get all transactions, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var transactions = await _transactionService.GetAllAsync(cancellationToken);
        return Ok(transactions);
    }

    /// <summary>Get a single transaction by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await _transactionService.GetByIdAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    /// <summary>
    /// Update a transaction's category, description, or amount.
    /// This is the primary editing flow after import.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _transactionService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Delete a transaction.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _transactionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
