using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.App.Application.Interfaces;

namespace MotoNomad.App.Infrastructure.Auth;

/// <summary>
/// Custom authentication state provider for Supabase Auth integration.
/// Manages user authentication state and provides claims-based identity.
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ISupabaseClientService _supabaseClient;
  private readonly ILogger<CustomAuthenticationStateProvider> _logger;
    private bool _isInitialized;

    public CustomAuthenticationStateProvider(
ISupabaseClientService supabaseClient,
        ILogger<CustomAuthenticationStateProvider> logger)
    {
 _supabaseClient = supabaseClient;
        _logger = logger;
        
        // Subscribe to Supabase auth state changes
 InitializeAuthStateListener();
    }

    /// <summary>
    /// Initializes listener for Supabase auth state changes.
    /// </summary>
    private void InitializeAuthStateListener()
    {
        try
        {
            if (!_supabaseClient.IsInitialized)
      {
     _logger.LogWarning("Supabase client not initialized, cannot subscribe to auth state changes");
     return;
    }

     var client = _supabaseClient.GetClient();
          
  // Subscribe to auth state changes
      client.Auth.AddStateChangedListener(OnAuthStateChanged);
            _logger.LogInformation("Subscribed to Supabase auth state changes");
     _isInitialized = true;
        }
   catch (Exception ex)
        {
  _logger.LogError(ex, "Failed to initialize auth state listener");
        }
    }

 /// <summary>
    /// Handler for Supabase auth state changes.
    /// </summary>
    private void OnAuthStateChanged(object? sender, Supabase.Gotrue.Constants.AuthState state)
    {
        _logger.LogInformation("Auth state changed: {State}", state);
        NotifyAuthenticationStateChanged();
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
         
   // Try to restore session if not already authenticated
  if (client.Auth.CurrentSession == null)
       {
                _logger.LogDebug("No current session, attempting to restore from storage");
await client.Auth.RetrieveSessionAsync();
            }
            
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
    /// Should be called after login or logout operations.
    /// </summary>
    public void NotifyAuthenticationStateChanged()
    {
  _logger.LogDebug("Notifying authentication state changed");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

  public void Dispose()
    {
        try
        {
      if (_isInitialized && _supabaseClient.IsInitialized)
      {
 var client = _supabaseClient.GetClient();
         client.Auth.RemoveStateChangedListener(OnAuthStateChanged);
         _logger.LogInformation("Unsubscribed from Supabase auth state changes");
  }
        }
   catch (Exception ex)
      {
       _logger.LogError(ex, "Error disposing CustomAuthenticationStateProvider");
        }
    }
}
