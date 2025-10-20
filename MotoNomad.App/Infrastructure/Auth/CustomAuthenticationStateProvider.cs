using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.App.Application.Interfaces;

namespace MotoNomad.App.Infrastructure.Auth;

/// <summary>
/// Custom authentication state provider for Supabase Auth integration.
/// Manages user authentication state and provides claims-based identity.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
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
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();
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

            // Add display_name from user metadata if available
            if (user.UserMetadata?.ContainsKey("display_name") == true)
            {
                var displayName = user.UserMetadata["display_name"]?.ToString();
                if (!string.IsNullOrEmpty(displayName))
                {
                    claims.Add(new Claim("display_name", displayName));
                }
            }

            var identity = new ClaimsIdentity(claims, "supabase");
            var principal = new ClaimsPrincipal(identity);

            _logger.LogDebug("User authenticated: {UserId}", user.Id);
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
    /// Should be called after login or logout operations.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
        _logger.LogDebug("Notifying authentication state changed");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
