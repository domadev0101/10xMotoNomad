namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when specified model is not found
/// </summary>
public class OpenRouterModelNotFoundException : OpenRouterException
{
  /// <summary>
    /// Initializes a new instance of OpenRouterModelNotFoundException
    /// </summary>
    /// <param name="message">Error message</param>
    public OpenRouterModelNotFoundException(string message) : base(message)
    {
    }
}
