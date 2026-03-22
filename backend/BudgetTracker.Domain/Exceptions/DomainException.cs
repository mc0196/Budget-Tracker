namespace BudgetTracker.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated inside the Domain layer.
/// Caught by the API's exception-handling middleware and translated to
/// an HTTP 400 Bad Request response.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
