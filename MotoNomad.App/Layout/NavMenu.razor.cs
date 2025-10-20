using Microsoft.AspNetCore.Components;
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
    /// Handles user logout action.
    /// Logs out the user, displays a success message, and redirects to login page.
    /// </summary>
    private async Task HandleLogout()
    {
        try
        {
            await AuthService.LogoutAsync();
            Snackbar.Add("Successfully logged out!", Severity.Success);
            NavigationManager.NavigateTo("/login");
        }
        catch (Exception ex)
        {
            Snackbar.Add("An error occurred during logout.", Severity.Error);
            // TODO: Logging
        }
    }
}
