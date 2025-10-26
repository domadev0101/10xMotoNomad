namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Base exception for all OpenRouter-related errors
/// </summary>
public class OpenRouterException : Exception
{
    /// <summary>
    /// Initializes a new instance of OpenRouterException
    /// </summary>
    /// <param name="message">Error message</param>
    public OpenRouterException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of OpenRouterException with an inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception that caused this exception</param>
    public OpenRouterException(string message, Exception innerException)
        : base(message, innerException)
    {
}
}
