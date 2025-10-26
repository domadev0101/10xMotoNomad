namespace MotoNomad.App.Application.Exceptions;

/// <summary>
/// Thrown when rate limit is exceeded
/// </summary>
public class OpenRouterRateLimitException : OpenRouterException
{
 /// <summary>
    /// Time to wait before retrying
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    /// <summary>
    /// Initializes a new instance of OpenRouterRateLimitException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="retryAfter">Optional time to wait before retrying</param>
    public OpenRouterRateLimitException(string message, TimeSpan? retryAfter = null)
     : base(message)
    {
     RetryAfter = retryAfter;
    }
}
