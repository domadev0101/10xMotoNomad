namespace MotoNomad.Application.Exceptions;

/// <summary>
/// Exception thrown when database operations fail.
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// Gets the database operation that failed.
    /// </summary>
    public string? Operation { get; }

    public DatabaseException(string message) : base(message)
    {
    }

    public DatabaseException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public DatabaseException(string operation, string message) 
        : base(message)
    {
        Operation = operation;
    }

    public DatabaseException(string operation, string message, Exception innerException) 
        : base(message, innerException)
    {
        Operation = operation;
    }
}
