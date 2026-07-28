namespace OCRWeb.Document.Domain.Exceptions;

/// <summary>
/// Thrown when a domain business rule is violated.
/// </summary>
public class DomainValidationException(string message) : Exception(message);
