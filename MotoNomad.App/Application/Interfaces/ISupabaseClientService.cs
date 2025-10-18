using Supabase;

namespace MotoNomad.App.Application.Interfaces;

/// <summary>
/// Interface for Supabase client service that provides access to Supabase client instance
/// </summary>
public interface ISupabaseClientService
{
    /// <summary>
    /// Gets the initialized Supabase client instance
    /// </summary>
    /// <returns>Configured Supabase client</returns>
    /// <exception cref="InvalidOperationException">Thrown when client is not initialized</exception>
    Client GetClient();

    /// <summary>
    /// Initializes the Supabase client connection
    /// This should be called during application startup
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Checks if the client is initialized
    /// </summary>
    bool IsInitialized { get; }
}
