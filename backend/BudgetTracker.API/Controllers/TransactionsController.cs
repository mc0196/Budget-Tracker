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

    /// <summary>Get transactions. If year+month are provided, returns only that month; otherwise returns all.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken cancellationToken)
    {
        var transactions = year.HasValue && month.HasValue
            ? await _transactionService.GetByMonthAsync(year.Value, month.Value, cancellationToken)
            : await _transactionService.GetAllAsync(cancellationToken);
        return Ok(transactions);
    }

    /// <summary>Create a manual transaction.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _transactionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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
