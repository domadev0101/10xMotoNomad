namespace MotoNomad.Application.Exceptions;

/// <summary>
/// Exception thrown when authentication or registration operations fail.
/// </summary>
public class AuthException : Exception
{
    /// <summary>
    /// Gets the authentication error code if available.
    /// </summary>
    public string? ErrorCode { get; }

    public AuthException(string message) : base(message)
    {
    }

    public AuthException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public AuthException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AuthException(string message, string errorCode, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
