namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when response validation fails
/// </summary>
public class OpenRouterResponseValidationException : OpenRouterException
{
    /// <summary>
    /// Expected JSON schema
    /// </summary>
    public string? ExpectedSchema { get; }

    /// <summary>
    /// Actual response received
    /// </summary>
    public string? ActualResponse { get; }

    /// <summary>
    /// Initializes a new instance of OpenRouterResponseValidationException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="expectedSchema">Expected JSON schema</param>
    /// <param name="actualResponse">Actual response received</param>
    public OpenRouterResponseValidationException(
        string message,
        string? expectedSchema = null,
        string? actualResponse = null)
        : base(message)
    {
        ExpectedSchema = expectedSchema;
        ActualResponse = actualResponse;
    }
}
