namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when OpenRouter API experiences server errors
/// </summary>
public class OpenRouterServerException : OpenRouterException
{
    /// <summary>
    /// Initializes a new instance of OpenRouterServerException
    /// </summary>
    /// <param name="message">Error message</param>
    public OpenRouterServerException(string message) : base(message)
    {
    }
}
