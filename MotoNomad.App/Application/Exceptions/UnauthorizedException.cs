namespace MotoNomad.Application.Exceptions;

/// <summary>
/// Exception thrown when user is not authenticated or lacks permission to access a resource.
/// </summary>
public class UnauthorizedException : Exception
{
    /// <summary>
    /// Gets the resource the user attempted to access.
    /// </summary>
    public string? Resource { get; }

    public UnauthorizedException()
        : base("You are not authorized to perform this action.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public UnauthorizedException(string resource, string message)
        : base(message)
    {
        Resource = resource;
    }
}
