using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Infrastructure.Auth;
using MudBlazor;

namespace MotoNomad.App.Shared;

/// <summary>
/// Component displaying user login status in the app bar.
/// Shows user greeting and logout button for authenticated users.
/// Shows login/register buttons for unauthenticated users with responsive design.
/// </summary>
public partial class LoginDisplay
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    /// <summary>
    /// Gets the display name from the authentication state.
    /// Returns display_name claim if available, otherwise email prefix, or "User" as fallback.
    /// </summary>
    /// <param name="authState">Current authentication state</param>
    /// <returns>User's display name</returns>
    private string GetDisplayName(AuthenticationState authState)
    {
        var user = authState.User;
        var displayName = user.FindFirst("display_name")?.Value;
        var email = user.FindFirst("email")?.Value;
        
        return displayName ?? email?.Split('@')[0] ?? "User";
    }

    /// <summary>
    /// Handles user logout action.
    /// Logs out the user, displays a success message, and redirects to login page.
    /// </summary>
    private async Task HandleLogout()
    {
        try
        {
            await AuthService.LogoutAsync();

            // Notify authentication state changed
            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
            }

            Snackbar.Add("Successfully logged out!", Severity.Success);

            // Normal SPA navigation (no full page reload)
            NavigationManager.NavigateTo("login");
        }
        catch (Exception ex)
        {
            Snackbar.Add("An error occurred during logout.", Severity.Error);
            // TODO: Logging
        }
    }
}
