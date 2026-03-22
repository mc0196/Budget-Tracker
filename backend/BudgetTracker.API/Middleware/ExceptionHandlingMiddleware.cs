using System.Net;
using System.Text.Json;
using BudgetTracker.Domain.Exceptions;

namespace BudgetTracker.API.Middleware;

/// <summary>
/// Global exception handler. Catches exceptions from any part of the pipeline
/// and converts them into consistent JSON error responses.
///
/// Why middleware instead of filters?
/// Middleware catches errors from ALL sources, including routing and model binding.
/// Filters only run after action selection.
///
/// Response format follows RFC 7807 "Problem Details" to be standards-compliant.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            DomainException           => (HttpStatusCode.BadRequest, "Business Rule Violation"),
            NotSupportedException     => (HttpStatusCode.UnsupportedMediaType, "Unsupported File Format"),
            ArgumentException         => (HttpStatusCode.BadRequest, "Invalid Argument"),
            _                         => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        var problemDetails = new
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            Detail = exception.Message,
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
}
