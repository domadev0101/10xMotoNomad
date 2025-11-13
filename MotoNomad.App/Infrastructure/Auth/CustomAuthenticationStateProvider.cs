using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.App.Infrastructure.Auth;

/// <summary>
/// Custom authentication state provider for Supabase Auth integration.
/// Manages user authentication state and provides claims-based identity.
/// Authentication state changes are triggered manually by AuthService after login/logout
/// to ensure profile data (display_name) is fully loaded before UI updates.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(
        ISupabaseClientService supabaseClient,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authentication state from Supabase.
    /// Returns authenticated user with claims if logged in, otherwise returns anonymous user.
    /// Fetches display_name from profiles table for accurate user information.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();

            // Session is restored during SupabaseClientService.InitializeAsync()
            // so CurrentUser should be available if user was logged in
            var user = client.Auth.CurrentUser;

            if (user == null)
            {
                _logger.LogDebug("No authenticated user found");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("email", user.Email ?? string.Empty),
            };

            // Fetch display_name from profiles table (more reliable than user metadata)
            try
            {
                var profile = await client
                    .From<Profile>()
                    .Select("*")
                    .Filter("id", Postgrest.Constants.Operator.Equals, user.Id)
                    .Single();

                if (profile != null && !string.IsNullOrWhiteSpace(profile.DisplayName))
                {
                    claims.Add(new Claim("display_name", profile.DisplayName));
                    _logger.LogDebug("Added display_name claim: {DisplayName}", profile.DisplayName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch profile for user {UserId}, continuing without display_name", user.Id);
                // Continue without display_name claim - not critical
            }

            var identity = new ClaimsIdentity(claims, "supabase");
            var principal = new ClaimsPrincipal(identity);

            _logger.LogInformation("User authenticated: {UserId} ({Email})", user.Id, user.Email);
            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Notifies the application that the authentication state has changed.
    /// Should be called by AuthService after login or logout operations to ensure
    /// profile data is fully loaded before UI update.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        _logger.LogDebug("Notifying authentication state changed");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void Dispose()
    {
        // No listener to unsubscribe - authentication state is managed manually by AuthService
    }
}
