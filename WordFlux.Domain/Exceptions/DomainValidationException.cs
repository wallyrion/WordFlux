namespace WordFlux.Domain.Exceptions;

public class DomainValidationException(string message, string? propertyName = null) : Exception(message)
{
    public string? PropertyName { get; } = propertyName;
}