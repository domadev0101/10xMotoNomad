namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when account has insufficient credits
/// </summary>
public class OpenRouterInsufficientCreditsException : OpenRouterException
{
    /// <summary>
    /// Initializes a new instance of OpenRouterInsufficientCreditsException
    /// </summary>
    /// <param name="message">Error message</param>
    public OpenRouterInsufficientCreditsException(string message) : base(message)
    {
    }
}
