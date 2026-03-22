using BudgetTracker.Application.DTOs;
using BudgetTracker.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetTracker.API.Controllers;

/// <summary>
/// Handles CSV/Excel file uploads for transaction import.
///
/// Why multipart/form-data?
/// Files can be large. Streaming via multipart avoids loading the entire
/// file into memory as a string. ASP.NET Core handles this via IFormFile.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImportController : ControllerBase
{
    private readonly TransactionImportService _importService;
    private readonly ILogger<ImportController> _logger;

    private static readonly HashSet<string> AllowedExtensions = [".csv", ".xls", ".xlsx"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public ImportController(TransactionImportService importService, ILogger<ImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a bank statement file (CSV or Excel).
    /// Returns the number of rows imported and skipped.
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(MaxFileSizeBytes)]
    [ProducesResponseType(typeof(ImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromQuery] Guid? accountId = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new ProblemDetails { Title = "No file uploaded.", Status = 400 });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new ProblemDetails
            {
                Title = "File too large.",
                Detail = $"Maximum allowed size is {MaxFileSizeBytes / 1024 / 1024} MB.",
                Status = 400
            });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, new ProblemDetails
            {
                Title = "Unsupported file format.",
                Detail = $"Allowed formats: {string.Join(", ", AllowedExtensions)}",
                Status = 415
            });

        _logger.LogInformation("Importing file: {FileName} ({Size} bytes)", file.FileName, file.Length);

        await using var stream = file.OpenReadStream();
        var result = await _importService.ImportAsync(stream, file.FileName, accountId, cancellationToken);

        if (result.IsDuplicate)
            return Conflict(new ProblemDetails
            {
                Title = "Duplicate import.",
                Detail = result.Message,
                Status = 409
            });

        return Ok(result);
    }
}
