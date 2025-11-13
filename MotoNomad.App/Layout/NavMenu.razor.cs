using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.Interfaces;
using MudBlazor;

namespace MotoNomad.App.Layout;

/// <summary>
/// Navigation menu component with authorization-based menu items.
/// </summary>
public partial class NavMenu
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    /// <summary>
    /// Handles logo click navigation.
    /// Redirects to home page (/) for both authenticated and non-authenticated users.
    /// </summary>
    private void HandleLogoClick()
    {
        // Always redirect to home page (/) - dashboard for authenticated, marketing for non-authenticated
        NavigationManager.NavigateTo("");
    }

    /// <summary>
    /// Handles user logout action.
    /// Logs out the user, displays a success message, and redirects to login page.
    /// Authentication state change is handled automatically by CustomAuthenticationStateProvider listener.
    /// </summary>
    private async Task HandleLogout()
    {
        try
        {
            await AuthService.LogoutAsync();

            Snackbar.Add("Successfully logged out!", Severity.Success);

            // Redirect to login page
            NavigationManager.NavigateTo("login");
        }
        catch (Exception ex)
        {
            Snackbar.Add("An error occurred during logout.", Severity.Error);
            // TODO: Logging
        }
    }
}
