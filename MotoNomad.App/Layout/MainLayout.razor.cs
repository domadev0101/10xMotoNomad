using Microsoft.AspNetCore.Components;
using MotoNomad.Application.Interfaces;
using MudBlazor;

namespace MotoNomad.App.Layout;

/// <summary>
/// Main application layout with MudBlazor components.
/// Includes app bar, navigation drawer, and inactivity timer for automatic logout.
/// </summary>
public partial class MainLayout : IDisposable
{
    [Inject] private IAuthService AuthService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private bool _drawerOpen = true;
    private System.Timers.Timer? _inactivityTimer;
    private const int InactivityTimeoutMinutes = 15;

    protected override void OnInitialized()
    {
        InitializeInactivityTimer();
    }

    /// <summary>
    /// Handles logo click navigation in AppBar.
    /// Redirects to home page (/) for both authenticated and non-authenticated users.
    /// Also resets the inactivity timer.
    /// </summary>
    private async Task HandleLogoClick()
    {
        // Always redirect to home page (/) - dashboard for authenticated, marketing for non-authenticated
        NavigationManager.NavigateTo("");

        ResetInactivityTimer();
    }

    /// <summary>
    /// Toggles the navigation drawer open/closed state and resets inactivity timer.
    /// </summary>
    private void ToggleDrawer()
    {
        _drawerOpen = !_drawerOpen;
        ResetInactivityTimer();
    }

    /// <summary>
    /// Initializes the inactivity timer that automatically logs out users after 15 minutes of inactivity.
    /// </summary>
    private void InitializeInactivityTimer()
    {
        _inactivityTimer = new System.Timers.Timer(InactivityTimeoutMinutes * 60 * 1000);
        _inactivityTimer.Elapsed += async (sender, args) =>
        {
            await HandleInactivityTimeout();
        };
        _inactivityTimer.Start();
    }

    /// <summary>
    /// Resets the inactivity timer, extending the session.
    /// </summary>
    private void ResetInactivityTimer()
    {
        _inactivityTimer?.Stop();
        _inactivityTimer?.Start();
    }

    /// <summary>
    /// Handles inactivity timeout by logging out the user and redirecting to login page.
    /// </summary>
    private async Task HandleInactivityTimeout()
    {
        if (await AuthService.IsAuthenticatedAsync())
        {
            await AuthService.LogoutAsync();
            await InvokeAsync(() =>
            {
                Snackbar.Add("Session expired due to inactivity. Please log in again.", Severity.Warning);
                NavigationManager.NavigateTo("login");
            });
        }
    }

    /// <summary>
    /// Disposes the inactivity timer to free resources.
    /// </summary>
    public void Dispose()
    {
        _inactivityTimer?.Stop();
        _inactivityTimer?.Dispose();
    }
}
