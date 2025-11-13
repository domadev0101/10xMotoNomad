using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.App.Application.Interfaces;
using Supabase.Gotrue;

namespace MotoNomad.App.Infrastructure.Auth;

/// <summary>
/// Mock authentication state provider for development/testing purposes.
/// Simulates a logged-in user without requiring actual Supabase authentication.
/// ?? WARNING: This should NEVER be used in production!
/// </summary>
public class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly string _mockUserId;
    private readonly string _mockEmail;
    private readonly string _mockDisplayName;
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<MockAuthenticationStateProvider> _logger;

    /// <summary>
    /// Creates a mock authentication provider with specified user credentials.
    /// </summary>
    /// <param name="userId">Mock user ID (should match a real user in your Supabase database)</param>
    /// <param name="email">Mock user email</param>
    /// <param name="displayName">Mock user display name</param>
    /// <param name="supabaseClient">Supabase client service to set mock session</param>
    /// <param name="logger">Logger for diagnostic messages</param>
    public MockAuthenticationStateProvider(
        ISupabaseClientService supabaseClient,
        ILogger<MockAuthenticationStateProvider> logger,
        string userId = "00000000-0000-0000-0000-000000000000",
        string email = "test@example.com",
        string displayName = "Test User")
    {
        _mockUserId = userId;
        _mockEmail = email;
        _mockDisplayName = displayName;
        _supabaseClient = supabaseClient;
        _logger = logger;

        // Set mock session in Supabase client
        SetMockSupabaseSession();
    }

    /// <summary>
    /// Returns a mock authenticated user with predefined claims.
    /// This simulates a logged-in state without connecting to Supabase.
    /// </summary>
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _mockUserId),
            new Claim("email", _mockEmail),
            new Claim("display_name", _mockDisplayName),
        };

        var identity = new ClaimsIdentity(claims, "mock");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        return Task.FromResult(authState);
    }

    /// <summary>
    /// Mock implementation of state change notification.
    /// Does nothing as the mock user never changes.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Sets a mock session in Supabase client to make services work with mock auth.
    /// Creates a fake User and Session objects.
    /// </summary>
    private void SetMockSupabaseSession()
    {
        try
        {
            var client = _supabaseClient.GetClient();

            // Create mock Supabase user
            var mockUser = new User
            {
                Id = _mockUserId,
                Email = _mockEmail,
                CreatedAt = DateTime.UtcNow
            };

            // Create mock session
            var mockSession = new Session
            {
                AccessToken = "mock_access_token",
                RefreshToken = "mock_refresh_token",
                ExpiresIn = 36000,
                TokenType = "bearer",
                User = mockUser
            };

            // Set current session and user in Supabase client
            client.Auth.SetSession(mockSession.AccessToken, mockSession.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set mock Supabase session for user {Email}", _mockEmail);
        }
    }
}
