using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MotoNomad.Application.DTOs.Profiles;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Infrastructure.Auth;
using MudBlazor;

namespace MotoNomad.App.Pages.Profiles;

/// <summary>
/// User profile page displaying current user information.
/// </summary>
public partial class Profile : ComponentBase
{
    [Inject]
    private IProfileService ProfileService { get; set; } = default!;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    [Inject]
    private ILogger<Profile> Logger { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private ProfileDto? _profile;
    private bool _isLoading = true;
    private string? _errorMessage;

    /// <summary>
    /// Initializes the component and loads user profile.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await LoadProfileAsync();
    }

    /// <summary>
    /// Loads current user profile from the service.
    /// </summary>
    private async Task LoadProfileAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            _profile = await ProfileService.GetCurrentProfileAsync();
            Logger.LogInformation("Profile loaded successfully for user {UserId}", _profile.Id);
        }
        catch (UnauthorizedException)
        {
            _errorMessage = "Please log in to view your profile.";
            Logger.LogWarning("Unauthorized attempt to access profile");
        }
        catch (NotFoundException ex)
        {
            _errorMessage = "Profile not found. Please contact support.";
            Logger.LogError(ex, "Profile not found");
        }
        catch (DatabaseException ex)
        {
            _errorMessage = "Failed to load profile. Please try again later.";
            Logger.LogError(ex, "Database error loading profile");
        }
        catch (Exception ex)
        {
            _errorMessage = "An unexpected error occurred. Please try again.";
            Logger.LogError(ex, "Unexpected error loading profile");
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Opens the edit profile dialog.
    /// </summary>
    private async Task OpenEditDialog()
    {
        if (_profile == null)
        {
            return;
        }

        var parameters = new DialogParameters<Shared.Dialogs.EditProfileDialog>
        {
            { x => x.Profile, _profile }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<Shared.Dialogs.EditProfileDialog>(
            "Edit Profile",
            parameters,
            options);

        var result = await dialog.Result;

        // Reload profile if dialog was not canceled
        if (result != null && !result.Canceled)
        {
            Logger.LogInformation("Reloading profile after successful edit");
            await LoadProfileAsync();
            
            // Notify authentication state changed to update LoginDisplay
            if (AuthStateProvider is CustomAuthenticationStateProvider customProvider)
            {
                customProvider.NotifyAuthenticationStateChanged();
                Logger.LogInformation("Notified authentication state changed after profile reload");
            }
            
            StateHasChanged(); // Force UI update
        }
    }
}
