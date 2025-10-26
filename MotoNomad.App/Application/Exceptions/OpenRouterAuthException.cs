namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when API authentication fails
/// </summary>
public class OpenRouterAuthException : OpenRouterException
{
    /// <summary>
    /// Initializes a new instance of OpenRouterAuthException
    /// </summary>
    /// <param name="message">Error message</param>
    public OpenRouterAuthException(string message) : base(message)
    {
}
}
