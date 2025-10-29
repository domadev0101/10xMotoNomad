using Supabase;
using Supabase.Gotrue;
using SupabaseClient = Supabase.Client;
using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.E2ETests.Configuration;

/// <summary>
/// Wrapper for Supabase client used in E2E tests.
/// Handles authentication and provides access to database operations.
/// </summary>
public class SupabaseTestClient : IAsyncDisposable
{
    private readonly SupabaseClient _client;
    private readonly TestConfiguration _config;

    public SupabaseClient Client => _client;

    public SupabaseTestClient(TestConfiguration config)
    {
        _config = config;

        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false // E2E tests don't need realtime
        };

        _client = new SupabaseClient(_config.SupabaseUrl, _config.SupabasePublicKey, options);
    }

    /// <summary>
    /// Initializes the Supabase client. Must be called before using the client.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    /// <summary>
    /// Logs in as User A (primary test user).
    /// </summary>
    public async Task<Session> LoginAsUserAAsync()
    {
        var response = await _client.Auth.SignIn(_config.UserAEmail, _config.UserAPassword);

        if (response?.User == null)
        {
            throw new InvalidOperationException("Failed to login as User A. Check credentials.");
        }

        return response;
    }

    /// <summary>
    /// Logs in as User B (for RLS/security tests).
    /// </summary>
    public async Task<Session> LoginAsUserBAsync()
    {
        var response = await _client.Auth.SignIn(_config.UserBEmail, _config.UserBPassword);

        if (response?.User == null)
        {
            throw new InvalidOperationException("Failed to login as User B. Check credentials.");
        }

        return response;
    }

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    public async Task SignOutAsync()
    {
        await _client.Auth.SignOut();
    }

    /// <summary>
    /// Gets the currently authenticated user.
    /// </summary>
    public User? CurrentUser => _client.Auth.CurrentUser;

    /// <summary>
    /// Checks if a user is currently authenticated.
    /// </summary>
    public bool IsAuthenticated => _client.Auth.CurrentUser != null;

    /// <summary>
    /// Cleans up test data for the specified user.
    /// Should be called in teardown to remove trips/companions created during tests.
    /// </summary>
    public async Task CleanupUserDataAsync(string userId)
    {
        // Must be authenticated to delete (RLS policy enforcement)
        if (!IsAuthenticated)
        {
            throw new InvalidOperationException("Must be authenticated to cleanup data.");
        }

        try
        {
            // Delete all trips for this user (companions will cascade delete)
            await _client
                .From<Trip>()
                .Where(t => t.UserId == Guid.Parse(userId))
                .Delete();
        }
        catch (Exception ex)
        {
            // Log but don't fail - cleanup is best effort
            Console.WriteLine($"Warning: Failed to cleanup data for user {userId}: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleans up ALL test data from both test users.
    /// Use with caution - only in global teardown.
    /// </summary>
    public async Task CleanupAllTestDataAsync()
    {
        try
        {
            // Login as User A and cleanup
            await LoginAsUserAAsync();
            if (CurrentUser != null)
            {
                await CleanupUserDataAsync(CurrentUser.Id);
            }
            await SignOutAsync();

            // Login as User B and cleanup
            await LoginAsUserBAsync();
            if (CurrentUser != null)
            {
                await CleanupUserDataAsync(CurrentUser.Id);
            }
            await SignOutAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to cleanup all test data: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Sign out if still authenticated
        if (IsAuthenticated)
        {
            await SignOutAsync();
        }

        // Dispose client if it implements IDisposable
        if (_client is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}