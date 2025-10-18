namespace MotoNomad.Application.Exceptions;

/// <summary>
/// Exception thrown when business validation rules are violated.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Gets the validation errors dictionary (field name -> error messages).
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }

    public ValidationException(string message, Dictionary<string, string[]> validationErrors) 
        : base(message)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(Dictionary<string, string[]> validationErrors) 
        : base("One or more validation errors occurred.")
    {
        ValidationErrors = validationErrors;
    }
}
